using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DelightCraft.Core.Chunk;
using DelightCraft.Infrastructure.Entity;
using DelightCraft.Infrastructure.Map;
using DelightCraft.Infrastructure.Property;
using UnityEngine;

namespace DelightCraft.Scripts
{
    /// <summary>
    /// 地形の生成とレンダリング処理
    /// 結構難易度高い設計なので、興味のある人向け
    /// </summary>
    public class TerrainClient : MonoBehaviour
    {
        // 定数
        private static readonly int blockObjectPoolAmount                = 300;
        private static readonly Vector3Int createBlockPosition           = new Vector3Int(0, -3000, 0);

        // フィールド
        [SerializeField] private Camera              mainCamera          = null;    // プレイヤーが使っているメインのカメラ
        [SerializeField] private GameObject          blockPrimitive      = null;    // ブロックのprimitive
        [SerializeField] private GameObject          blockColliderObject = null;    // ブロックの当たり判定を持つコライダーオブジェクト
        [SerializeField] private Material            blockTestMaterial   = null;    // ブロックの生成テスト用のマテリアル
        [SerializeField] private TileMapDefinition   tileMapDefinition   = null;    // タイル情報を格納したマップデータ
        [SerializeField] private PerlinNoiseProperty noiseProperty       = null;    // パーリンノイズのプロパティ
        [SerializeField] private float               redrawingThreshold  = 1.0f;    // 再描画を実行させる閾値(カメラがどれだけ動いたか)

        // 内部保持変数
        private readonly HashSet<Chunk>              loadedChunks        = new HashSet<Chunk>();                     // ロード済みチャンクリスト
        private readonly HashSet<GameObject>         blockObjectPool     = new HashSet<GameObject>();                // 作成済みのフェイシャルブロックオブジェクトプール
        private readonly Dictionary<Vector3Int, GameObject> collisionMap = new Dictionary<Vector3Int, GameObject>(); // 物理判定用のブロックマップ

        // 前回描画フレームのカメラ位置
        private Vector3    beforeRenderedFrameCameraPosition             = Vector3.zero;
        // 前回描画フレームのカメラ回転
        private Vector3    beforeRenderedFrameCameraRotation             = Vector3.zero;
        // 初期描画済み？
        private bool       isInitializeRendered                          = false;
        // ブロックのプール先
        private GameObject blockGroupParentObject = null;
        private GameObject collisionGroupParentObject = null;
        private MeshFilter blockGroupParentMesh   = null;


        /// <summary>
        /// 初期化処理
        /// </summary>
        private async void Start()
        {
            // ランダムなシード値に応じてチャンクを生成するためのファクトリのインスタンスを生成
            RandomChunkFactory randomChunkFactory = new RandomChunkFactory(noiseProperty, tileMapDefinition);

            // ファクトリからチャンクの生成をします。生成したチャンクはロード済みのチャンクとして保持します。
            {
                loadedChunks.Add(randomChunkFactory.Create(new Vector3Int(0,0,0)));
                loadedChunks.Add(randomChunkFactory.Create(new Vector3Int((int)noiseProperty.Size.x,0,0)));
                loadedChunks.Add(randomChunkFactory.Create(new Vector3Int(-1 * (int)noiseProperty.Size.x,0,0)));
                loadedChunks.Add(randomChunkFactory.Create(new Vector3Int((int)noiseProperty.Size.x,0,(int)noiseProperty.Size.y)));
                loadedChunks.Add(randomChunkFactory.Create(new Vector3Int((int)noiseProperty.Size.x,0,-1 * (int)noiseProperty.Size.y)));
                loadedChunks.Add(randomChunkFactory.Create(new Vector3Int(-1 * (int)noiseProperty.Size.x,0,-1 * (int)noiseProperty.Size.y)));
                loadedChunks.Add(randomChunkFactory.Create(new Vector3Int(-1 * (int)noiseProperty.Size.x,0,(int)noiseProperty.Size.y)));
                loadedChunks.Add(randomChunkFactory.Create(new Vector3Int(0, 0, -1 * (int) noiseProperty.Size.y)));
                loadedChunks.Add(randomChunkFactory.Create(new Vector3Int(0,0,(int)noiseProperty.Size.y)));
            }

            // 描画するブロックプールの配置先を作成します。
            blockGroupParentObject = new GameObject("BlockPool");
            blockGroupParentMesh   = blockGroupParentObject.AddComponent<MeshFilter>();

            // 全チャンクの全ブロックのコリジョンを生成
            await CreateBlockCollisions();

            // 全チャンクの全ブロックを描画するとかしたら、メモリがいくらあっても足りないので使い回すオブジェクトを生成しておく。
            await CreateBlockObjectPool(blockGroupParentObject.transform, blockObjectPoolAmount);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private async void Update()
        {
            if (blockGroupParentObject == null) return;
            if (!IsNeedToRenderableObject()) return;

            if (!isInitializeRendered) isInitializeRendered = true;
            beforeRenderedFrameCameraPosition = mainCamera.transform.position;
            beforeRenderedFrameCameraRotation = mainCamera.transform.eulerAngles;

            await MoveRenderingPosition(blockGroupParentObject.transform);
        }

        /// <summary>
        /// ブロックの物理判定を生成
        /// </summary>
        /// <returns></returns>
        private async Task CreateBlockCollisions()
        {
            collisionGroupParentObject = new GameObject("collisions");
            foreach (Chunk loadedChunk in loadedChunks)
            {
                foreach (Vector3Int blockMapKey in loadedChunk.BlockMap.Keys)
                {
                    GameObject blockObject = new GameObject("block");
                    blockObject.layer = 8;
                    blockObject.transform.localScale = Vector3.one;
                    blockObject.transform.position = createBlockPosition;
                    blockObject.transform.SetParent(collisionGroupParentObject.transform);
                    collisionMap.Add(blockMapKey, blockObject);
                }
            }
        }

        /// <summary>
        /// ブロックオブジェクトのプール
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private async Task<GameObject[]> CreateBlockObjectPool(Transform parent, int count = 0)
        {
            List<GameObject> createdObjects = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                GameObject blockObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blockObject.layer = 8;
                blockObject.transform.localScale = Vector3.one;
                blockObject.transform.position = createBlockPosition;
                blockObject.transform.SetParent(parent);
                createdObjects.Add(blockObject);
                blockObjectPool.Add(blockObject);
            }

            return createdObjects.ToArray();
        }

        /// <summary>
        /// 再度レンダリング対象を策定する必要があるかどうかを判定します。
        /// </summary>
        /// <returns></returns>
        private bool IsNeedToRenderableObject()
        {
            if (!isInitializeRendered) return true;

            if ((beforeRenderedFrameCameraPosition - mainCamera.transform.position).sqrMagnitude >
                redrawingThreshold * redrawingThreshold ||
                (beforeRenderedFrameCameraRotation - mainCamera.transform.eulerAngles).sqrMagnitude >
                redrawingThreshold * redrawingThreshold)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// レンダリングポジションにブロックを移動させます。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private async Task MoveRenderingPosition(Transform parent)
        {
            Stack<GameObject> nonUsedObjects = new Stack<GameObject>(blockObjectPool.ToArray());
            foreach (Chunk loadedChunk in loadedChunks)
            {
                var renderingPositions = loadedChunk.GetCoordinateNoneAdjacent(mainCamera);
                if (renderingPositions.Count == 0)
                {
                    continue;
                }

                if (nonUsedObjects.Count < renderingPositions.Count)
                {
                    GameObject[] createdObjects = await CreateBlockObjectPool(parent, (renderingPositions.Count - nonUsedObjects.Count) + 20);
                    foreach (GameObject createdObject in createdObjects)
                    {
                        nonUsedObjects.Push(createdObject);
                    }
                }

                foreach (KeyValuePair<Vector3Int,Color> blockData in renderingPositions)
                {
                    GameObject blockObject = nonUsedObjects.Pop();
                    blockObject.transform.position = blockData.Key;
                    blockObject.GetComponent<MeshRenderer>().material.color = blockData.Value;
                    blockObject.GetComponent<MeshRenderer>().enabled = true;
                }
            }
//
//            foreach (GameObject nonUsedObject in nonUsedObjects)
//            {
//                nonUsedObject.GetComponent<MeshRenderer>().enabled = false;
//                nonUsedObject.GetComponent<Collider>().enabled = false;
//            }
        }
    }
}