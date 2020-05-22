#define Unlit
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using B83.Image.BMP;
using MMD.PMX;
using Paloma;
using TextureSupport;
using UnityEngine;
using UnityEngine.Rendering;

namespace MMD
{
    public class PMXConverter : IDisposable
    {
        /// <summary>
        ///     アニメーションタイプ
        /// </summary>
        public enum AnimationType
        {
            GenericMecanim, //汎用アバターでのMecanim
            HumanMecanim, //人型アバターでのMecanim
            LegacyAnimation //旧式アニメーション
        }

        private const uint
            c_max_vertex_count_in_mesh =
                65535; //meshに含まれる最大頂点数(Unity3D的には65536迄入ると思われるが、ushort.MaxValueは特別な値として使うのでその分を除外)

        private static readonly BMPLoader BMPLoader = new BMPLoader();
        private PMXFormat format_;

        private GameObject root_game_object_;
        private float scale_;
        private bool use_ik_;

        /// <summary>
        ///     デフォルトコンストラクタ
        /// </summary>
        /// <remarks>
        ///     ユーザーに依るインスタンス作成を禁止する
        /// </remarks>
        private PMXConverter()
        {
        }

        /// <summary>
        ///     Disposeインターフェース
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        ///     GameObjectを作成する
        /// </summary>
        /// <param name='format'>内部形式データ</param>
        /// <param name='use_rigidbody'>剛体を使用するか</param>
        /// <param name='animation_type'>アニメーションタイプ</param>
        /// <param name='use_ik'>IKを使用するか</param>
        /// <param name='scale'>スケール</param>
        public static GameObject CreateGameObject(PMXFormat format, bool use_rigidbody, AnimationType animation_type,
            bool use_ik, float scale)
        {
            GameObject result;
            using (var converter = new PMXConverter())
            {
                result = converter.CreateGameObject_(format, use_rigidbody, animation_type, use_ik, scale);
            }

            return result;
        }

        /// <summary>
        ///     GameObjectを作成する
        /// </summary>
        /// <param name='format'>内部形式データ</param>
        /// <param name='use_rigidbody'>剛体を使用するか</param>
        /// <param name='animation_type'>アニメーションタイプ</param>
        /// <param name='use_ik'>IKを使用するか</param>
        /// <param name='scale'>スケール</param>
        private GameObject CreateGameObject_(PMXFormat format, bool use_rigidbody, AnimationType animation_type,
            bool use_ik, float scale)
        {
            format_ = format;
            use_ik_ = use_ik;
            scale_ = scale;
            root_game_object_ = new GameObject(format_.meta_header.name);

            try
            {
                var creationInfo = CreateMeshCreationInfo();
                var mesh = CreateMesh(creationInfo);
                var materials = CreateMaterials(creationInfo);

                SetMeshRenderer(mesh, materials);
                return root_game_object_;
            }
            catch (Exception e)
            {
                //UnityEngine.Object.DestroyImmediate(root_game_object_);
                Debug.LogError(e + " " + e.Message);
                return root_game_object_;
            }
        }

        /// <summary>
        ///     メッシュを作成する為の情報を作成
        /// </summary>
        /// <returns>メッシュ作成情報</returns>
        private MeshCreationInfo[] CreateMeshCreationInfo()
        {
            // 1メッシュで収まる場合でも-Multi()を使っても問題は起き無いが、
            // -Multi()では頂点数計測をマテリアル単位で行う関係上、頂点数が多く見積もられる(概算値)。
            // (1頂点を複数のマテリアルが参照している場合に参照している分だけ計上してしまう。)
            // 依って上限付近では本来1メッシュで収まる物が複数メッシュに分割されてしまう事が有るので注意。
            // 
            // -Multi()を使っても最終的には頂点数を最適化するので、
            // 1メッシュに収まってしまえば-Single()と同じ頂点数に為る(確定値)。
            // 
            // 単純に-Single()の方が解析が少ない分早い。

            MeshCreationInfo[] result;
            if (format_.vertex_list.vertex.Length < c_max_vertex_count_in_mesh)
                //1メッシュで収まるなら
                result = CreateMeshCreationInfoSingle();
            else
                //1メッシュで収まらず、複数メッシュに分割するなら
                result = CreateMeshCreationInfoMulti();
            return result;
        }

        /// <summary>
        ///     メッシュを作成する為の情報を作成(単体メッシュ版)
        /// </summary>
        /// <returns>メッシュ作成情報</returns>
        private MeshCreationInfo[] CreateMeshCreationInfoSingle()
        {
            MeshCreationInfo[] result = {new MeshCreationInfo()};
            //全マテリアルを設定
            result[0].value = CreateMeshCreationInfoPacks();
            //全頂点を設定
            result[0].all_vertices =
                Enumerable.Range(0, format_.vertex_list.vertex.Length).Select(x => (uint) x).ToArray();
            //頂点リアサインインデックス用辞書作成
            result[0].reassign_dictionary = new Dictionary<uint, uint>(result[0].all_vertices.Length);
            for (uint i = 0, i_max = (uint) result[0].all_vertices.Length; i < i_max; ++i)
                result[0].reassign_dictionary[i] = i;
            return result;
        }

        /// <summary>
        ///     全マテリアルをメッシュ作成情報のマテリアルパックとして返す
        /// </summary>
        /// <returns>メッシュ作成情報のマテリアルパック</returns>
        private MeshCreationInfo.Pack[] CreateMeshCreationInfoPacks()
        {
            uint plane_start = 0;
            //マテリアル単位のMeshCreationInfo.Packを作成する
            return Enumerable.Range(0, format_.material_list.material.Length)
                .Select(x =>
                {
                    var pack = new MeshCreationInfo.Pack();
                    pack.material_index = (uint) x;
                    var plane_count = format_.material_list.material[x].face_vert_count;
                    pack.plane_indices = format_.face_vertex_list.face_vert_index.Skip((int) plane_start)
                        .Take((int) plane_count)
                        .ToArray();
                    pack.vertices = pack.plane_indices.Distinct() //重複削除
                        .ToArray();
                    plane_start += plane_count;
                    return pack;
                })
                .ToArray();
        }

        /// <summary>
        ///     メッシュを作成する為の情報を作成(複数メッシュ版)
        /// </summary>
        /// <returns>メッシュ作成情報</returns>
        private MeshCreationInfo[] CreateMeshCreationInfoMulti()
        {
            //マテリアル単位のMeshCreationInfo.Packを作成する
            var packs = CreateMeshCreationInfoPacks();
            //マテリアル細分化
            packs = SplitSubMesh(packs);
            //頂点数の多い順に並べる(メッシュ分割アルゴリズム上、後半に行く程頂点数が少ない方が敷き詰め効率が良い)
            Array.Sort(packs, (x, y) => y.vertices.Length - x.vertices.Length);

            var result = new List<MeshCreationInfo>();
            do
            {
                uint vertex_sum = 0;
                var info = new MeshCreationInfo();
                //マテリアルパック作成
                info.value = Enumerable.Range(0, packs.Length)
                    .Where(x => null != packs[x]) //有効なマテリアルに絞る
                    .Where(x =>
                    {
                        //採用しても頂点数が限界を超えないなら
                        vertex_sum += (uint) packs[x].vertices.Length;
                        return vertex_sum < c_max_vertex_count_in_mesh;
                    })
                    .Select(x =>
                    {
                        //マテリアルの採用と無効化
                        var pack = packs[x];
                        packs[x] = null;
                        return pack;
                    })
                    .ToArray();
                //マテリアルインデックスに並べる(メッシュの選定が終わったので見易い様に並びを戻す)
                Array.Sort(info.value,
                    (x, y) => x.material_index > y.material_index ? 1 : x.material_index < y.material_index ? -1 : 0);
                //総頂点作成
                info.all_vertices = info.value.SelectMany(x => x.vertices).Distinct().ToArray();
                Array.Sort(info.all_vertices);
                //頂点リアサインインデックス用辞書作成
                info.reassign_dictionary = new Dictionary<uint, uint>();
                uint reassign_index = 0;
                foreach (var i in info.all_vertices) info.reassign_dictionary[i] = reassign_index++;
                //戻り値に追加
                result.Add(info);
            } while (packs.Any(x => null != x)); //使用していないマテリアルが為るならループ

            return result.ToArray();
        }

        /// <summary>
        ///     1マテリアルの頂点数が1メッシュで表現出来ない場合に分割する
        /// </summary>
        /// <returns>メッシュ作成情報のマテリアルパック</returns>
        /// <param name='creation_infos'>メッシュ作成情報のマテリアルパック</param>
        private MeshCreationInfo.Pack[] SplitSubMesh(MeshCreationInfo.Pack[] packs)
        {
            var result = packs;
            if (packs.Any(x => c_max_vertex_count_in_mesh <= x.vertices.Length))
            {
                //1メッシュに収まらないマテリアルが有るなら
                var result_list = new List<MeshCreationInfo.Pack>();
                foreach (var pack in packs)
                    if (c_max_vertex_count_in_mesh <= pack.vertices.Length)
                    {
                        //1メッシュに収まらないなら
                        //分離
                        var split_pack = SplitSubMesh(pack);
                        foreach (var i in split_pack) result_list.Add(i);
                    }
                    else
                    {
                        //1メッシュに収まるなら
                        //素通し
                        result_list.Add(pack);
                    }

                result = result_list.ToArray();
            }

            return result;
        }

        /// <summary>
        ///     1マテリアルの頂点数が1メッシュで表現出来ないので分割する
        /// </summary>
        /// <returns>メッシュ作成情報のマテリアルパック</returns>
        /// <param name='creation_infos'>メッシュ作成情報のマテリアルパック</param>
        private List<MeshCreationInfo.Pack> SplitSubMesh(MeshCreationInfo.Pack pack)
        {
            var result = new List<MeshCreationInfo.Pack>();
            //1メッシュに収まらないなら
            var plane_end = (uint) pack.plane_indices.Length;
            uint plane_start = 0;
            while (plane_start < plane_end)
            {
                //まだ面が有るなら
                uint plane_count = 0;
                uint vertex_count = 0;
                while (true)
                {
                    //現在の頂点数から考えると、余裕分の1/3迄の数の面は安定して入る
                    //はみ出て欲しいから更に1面(3頂点)を足す
                    plane_count += (c_max_vertex_count_in_mesh - vertex_count) / 3 * 3 + 3;
                    vertex_count = (uint) pack.plane_indices.Skip((int) plane_start) //面頂点インデックス取り出し(先頭)
                        .Take((int) plane_count) //面頂点インデックス取り出し(末尾)
                        .Distinct() //重複削除
                        .Count(); //個数取得
                    if (c_max_vertex_count_in_mesh <= vertex_count)
                    {
                        //1メッシュを超えているなら
                        //此処でのメッシュ超えは必ずc_max_vertex_count_in_meshぎりぎりで有り、1面(3頂点)を1つ取れば収まる様になっている
                        plane_count -= 3;
                        break;
                    }

                    if (plane_end <= plane_start + plane_count)
                        //面の最後なら
                        break;
                }

                //分離分を戻り値の追加
                var result_pack = new MeshCreationInfo.Pack();
                ;
                result_pack.material_index = pack.material_index;
                result_pack.plane_indices = pack.plane_indices.Skip((int) plane_start) //面頂点インデックス取り出し(先頭)
                    .Take((int) plane_count) //面頂点インデックス取り出し(末尾)
                    .ToArray();
                result_pack.vertices = result_pack.plane_indices.Distinct() //重複削除
                    .ToArray();
                result.Add(result_pack);
                //開始点を後ろに
                plane_start += plane_count;
            }

            return result;
        }

        /// <summary>
        ///     メッシュ作成
        /// </summary>
        /// <returns>メッシュ</returns>
        /// <param name='creation_info'>メッシュ作成情報</param>
        private Mesh[] CreateMesh(MeshCreationInfo[] creation_info)
        {
            var result = new Mesh[creation_info.Length];
            for (int i = 0, i_max = creation_info.Length; i < i_max; ++i)
            {
                var mesh = new Mesh();
                EntryAttributesForMesh(mesh, creation_info[i]);
                SetSubMesh(mesh, creation_info[i]);
                //CreateAssetForMesh(mesh, i);
                result[i] = mesh;
            }

            return result;
        }

        /// <summary>
        ///     メッシュに基本情報(頂点座標・法線・UV・ボーンウェイト)を登録する
        /// </summary>
        /// <param name='mesh'>対象メッシュ</param>
        /// <param name='creation_info'>メッシュ作成情報</param>
        private void EntryAttributesForMesh(Mesh mesh, MeshCreationInfo creation_info)
        {
            mesh.vertices = creation_info.all_vertices.Select(x => format_.vertex_list.vertex[x].pos * scale_)
                .ToArray();
            mesh.normals = creation_info.all_vertices.Select(x => format_.vertex_list.vertex[x].normal_vec).ToArray();
            mesh.uv = creation_info.all_vertices.Select(x => format_.vertex_list.vertex[x].uv).ToArray();
            if (0 < format_.header.additionalUV)
                //追加UVが1つ以上有れば
                //1つ目のみ登録
                mesh.uv2 = creation_info.all_vertices.Select(x => new Vector2(format_.vertex_list.vertex[x].add_uv[0].x,
                    format_.vertex_list.vertex[x].add_uv[0].y)).ToArray();
            mesh.boneWeights = creation_info.all_vertices
                .Select(x => ConvertBoneWeight(format_.vertex_list.vertex[x].bone_weight)).ToArray();
            mesh.colors = creation_info.all_vertices.Select(x =>
                    new Color(0.0f, 0.0f, 0.0f, format_.vertex_list.vertex[x].edge_magnification * 0.25f))
                .ToArray(); //不透明度にエッジ倍率を0.25倍した情報を仕込む(0～8迄は表せる)
        }

        /// <summary>
        ///     ボーンウェイトをUnity用に変換する
        /// </summary>
        /// <returns>Unity用ボーンウェイト</returns>
        /// <param name='bone_weight'>PMX用ボーンウェイト</param>
        private BoneWeight ConvertBoneWeight(PMXFormat.BoneWeight bone_weight)
        {
            //HACK: 取り敢えずボーンウェイトタイプを考えずにBDEFx系として登録する
            var result = new BoneWeight();
            switch (bone_weight.method)
            {
                case PMXFormat.Vertex.WeightMethod.BDEF1: goto case PMXFormat.Vertex.WeightMethod.BDEF4;
                case PMXFormat.Vertex.WeightMethod.BDEF2: goto case PMXFormat.Vertex.WeightMethod.BDEF4;
                case PMXFormat.Vertex.WeightMethod.BDEF4:
                    //BDEF4なら
                    result.boneIndex0 = (int) bone_weight.bone1_ref;
                    result.weight0 = bone_weight.bone1_weight;
                    result.boneIndex1 = (int) bone_weight.bone2_ref;
                    ;
                    result.weight1 = bone_weight.bone2_weight;
                    result.boneIndex2 = (int) bone_weight.bone3_ref;
                    result.weight2 = bone_weight.bone3_weight;
                    result.boneIndex3 = (int) bone_weight.bone4_ref;
                    result.weight3 = bone_weight.bone4_weight;
                    break;
                case PMXFormat.Vertex.WeightMethod.SDEF:
                    //SDEFなら
                    //HACK: BDEF4と同じ対応
                    goto case PMXFormat.Vertex.WeightMethod.BDEF4;
                case PMXFormat.Vertex.WeightMethod.QDEF:
                    //QDEFなら
                    //HACK: BDEF4と同じ対応
                    goto case PMXFormat.Vertex.WeightMethod.BDEF4;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        /// <summary>
        ///     メッシュにサブメッシュを登録する
        /// </summary>
        /// <param name='mesh'>対象メッシュ</param>
        /// <param name='creation_info'>メッシュ作成情報</param>
        private void SetSubMesh(Mesh mesh, MeshCreationInfo creation_info)
        {
            // マテリアル対サブメッシュ
            // サブメッシュとはマテリアルに適用したい面頂点データのこと
            // 面ごとに設定するマテリアルはここ
            mesh.subMeshCount = creation_info.value.Length;
            for (int i = 0, i_max = creation_info.value.Length; i < i_max; ++i)
            {
                //format_.face_vertex_list.face_vert_indexを[start](含む)から[start+count](含まず)迄取り出し
                var indices = creation_info.value[i].plane_indices
                    .Select(x => (int) creation_info.reassign_dictionary[x]) //頂点リアサインインデックス変換
                    .ToArray();
                mesh.SetTriangles(indices, i);
            }
        }

        /// <summary>
        ///     マテリアル作成
        /// </summary>
        /// <returns>マテリアル</returns>
        /// <param name='creation_info'>メッシュ作成情報</param>
        private Material[][] CreateMaterials(MeshCreationInfo[] creation_info)
        {
            // 適当なフォルダに投げる
            //string path = format_.meta_header.folder + "/Materials/";
            //if (!System.IO.Directory.Exists(path))
            //{
            //    AssetDatabase.CreateFolder(format_.meta_header.folder, "Materials");
            //}

            ////全マテリアルを作成
            var materials = EntryAttributesForMaterials();
            //CreateAssetForMaterials(materials);

            //メッシュ単位へ振り分け
            var result = new Material[creation_info.Length][];
            for (int i = 0, i_max = creation_info.Length; i < i_max; ++i)
                result[i] = creation_info[i].value.Select(x => materials[x.material_index]).ToArray();

            return result;
        }

        /// <summary>
        ///     マテリアルに基本情報(シェーダー・カラー・テクスチャ)を登録する
        /// </summary>
        /// <returns>マテリアル</returns>
        private Material[] EntryAttributesForMaterials()
        {
            return Enumerable.Range(0, format_.material_list.material.Length)
                .Select(x => new
                {
                    material_index = (uint) x,
                    is_transparent = false
                })
                .Select(x => ConvertMaterial(x.material_index, x.is_transparent))
                .ToArray();
        }

        /// <summary>
        ///     マテリアルをUnity用に変換する
        /// </summary>
        /// <returns>Unity用マテリアル</returns>
        /// <param name='material_index'>PMX用マテリアルインデックス</param>
        /// <param name='is_transparent'>透過か</param>
        private Material ConvertMaterial(uint material_index, bool is_transparent)
        {
            var material = format_.material_list.material[material_index];

            //先にテクスチャ情報を検索
            Texture2D main_texture = null;
            if (material.usually_texture_index == uint.MaxValue)
            {
                main_texture = Texture2D.whiteTexture;
            }
            else
            {
                var texture_file_name = format_.texture_list.texture_file[material.usually_texture_index];

                var path = format_.meta_header.folder + "/" + texture_file_name;
                if (File.Exists(path))
                {
                    if (material.usually_texture_index < format_.texture_list.texture_file.Length)
                    {
                        if (texture_file_name.EndsWith(".dds",StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                main_texture = DDSImage.LoadDDS(path);
                                main_texture.wrapModeV = TextureWrapMode.MirrorOnce;
                            }
                            catch
                            {
                                main_texture = Texture2D.whiteTexture;
                            }
                        }
                        else if (texture_file_name.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                        {
                            main_texture = BMPLoader.LoadBMP(path).ToTexture2D();
                        }
                        else if (texture_file_name.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var bitmap = TargaImage.LoadTargaImage(path);
                                main_texture = new Texture2D(bitmap.Width, bitmap.Height);

                                var stream = new MemoryStream();
                                bitmap.Save(stream, ImageFormat.Png);
                                main_texture.LoadImage(stream.ToArray());
                                stream.Close();
                                main_texture.Apply();
                            }
                            catch (Exception e)
                            {
                                Debug.Log("TGA Exception");
                                Debug.Log(texture_file_name);
                                Debug.LogError(e.Message);
                                main_texture = Texture2D.whiteTexture;
                            }

                            //main_texture = CreateTexture(path);
                        }
                        else
                        {
                            main_texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                            main_texture.LoadImage(File.ReadAllBytes(path)); // Fill the texture field 
                        }

                        //main_texture.alphaIsTransparency = true;
                    }
                }
                else
                {
                    main_texture = Texture2D.whiteTexture;
                }
            }

#if Unlit

            var result = new Material(Resources.Load<Material>("MMDUnlit"))
            {
                renderQueue = (int)(3000 + material_index)
            };
            result.SetTexture("_UnlitColorMap", main_texture);
            result.SetVector("_UnlitColor", material.diffuse_color);
            result.SetFloat("_TransparentSortPriority", material_index);
            result.name = format_.material_list.material[material_index].name;
            return result;

#else
            
            var result = new Material(Resources.Load<Material>("MMDLit"))
            {
                renderQueue = (int) (3000 + material_index)
            };
            result.SetTexture("_BaseColorMap", main_texture);
            result.SetVector("_BaseColor", material.diffuse_color);
            result.SetFloat("_TransparentSortPriority", material_index);
            result.name = format_.material_list.material[material_index].name;
            return result;

#endif
        }

        public void SetMeshRenderer(Mesh[] mesh, Material[][] materials) //, GameObject[] bones)
        {
            // メッシュルートを生成してルートの子供に付ける
            var mesh_root_transform = new GameObject("Mesh").transform;
            mesh_root_transform.parent = root_game_object_.transform;

            for (int i = 0, i_max = mesh.Length; i < i_max; ++i)
            {
                var mesh_transform = new GameObject("Mesh" + i).transform;
                mesh_transform.parent = mesh_root_transform;
                var mr = mesh_transform.gameObject.AddComponent<MeshRenderer>();
                mr.materials = materials[i];
                mr.receiveShadows = false;
                mr.shadowCastingMode = ShadowCastingMode.Off;

                var mf = mesh_transform.gameObject.AddComponent<MeshFilter>();
                mf.mesh = mesh[i];
            }
        }

        /// <summary>
        ///     メッシュを作成する時に参照するデータの纏め
        /// </summary>
        private class MeshCreationInfo
        {
            public uint[] all_vertices; //総頂点
            public Dictionary<uint, uint> reassign_dictionary; //頂点リアサインインデックス用辞書
            public Pack[] value;

            public class Pack
            {
                public uint material_index; //マテリアル
                public uint[] plane_indices; //面
                public uint[] vertices; //頂点
            }
        }
    }
}