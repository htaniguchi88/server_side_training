using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab;

    [SerializeField] private int chunkSize = 50;

    [SerializeField] private float noiseScale = .05f;

    [SerializeField, Range(0, 1)] private float threshold = .5f;

    [SerializeField] private Material material;

    [SerializeField] private bool sphere = false;

    private List<Mesh> meshes = new List<Mesh>();

    private void Start()
    {
        Generate();
    }

    private void Generate()
    {
        float startTime = Time.realtimeSinceStartup;

        #region Create Mesh Data

        List<CombineInstance> blockData = new List<CombineInstance>();
        MeshFilter blockMesh =
            Instantiate(blockPrefab, Vector3.zero, Quaternion.identity)
                .GetComponent<MeshFilter>();

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float noiseValue =
                        Perlin3D(x * noiseScale, y * noiseScale, z * noiseScale);
                    if (noiseValue >= threshold)
                    {
                        float raduis = chunkSize / 2;
                        if (sphere && Vector3.Distance(new Vector3(x, y, z), Vector3.one * raduis) > raduis)
                            continue;

                        blockMesh.transform.position = new Vector3(x, y, z);
                        CombineInstance ci = new CombineInstance
                        {
                            mesh = blockMesh.sharedMesh,
                            transform = blockMesh.transform.localToWorldMatrix,
                        };
                        blockData.Add(ci);
                    }
                }
            }
        }

        Destroy(blockMesh.gameObject);

        #endregion

        #region Separate Mesh Data

        List<List<CombineInstance>>
            blockDataLists =
                new List<List<CombineInstance>>();
        int vertexCount = 0;
        blockDataLists.Add(new List<CombineInstance>());
        for (int i = 0; i < blockData.Count; i++)
        {
            vertexCount += blockData[i].mesh.vertexCount; //keep track of total vertices
            if (vertexCount > 65536)
            {
                //if the list has reached it's capacity. if total vertex count is more then 65536, reset counter and start adding them to a new list.
                vertexCount = 0;
                blockDataLists.Add(new List<CombineInstance>());
                i--;
            }
            else
            {
                //if the list hasn't yet reached it's capacity. safe to add another block data to this list
                blockDataLists.Last().Add(blockData[i]); //the newest list will always be the last one added
            }
        }

        #endregion

        #region Create Mesh

        //the creation of the final mesh from the data.

        Transform container = new GameObject("Meshys").transform; //create container object
        foreach (List<CombineInstance> data in blockDataLists)
        {
            //for each list (of block data) in the list (of other lists)
            GameObject g = new GameObject("Meshy"); //create gameobject for the mesh
            g.transform.parent = container; //set parent to the container we just made
            MeshFilter mf = g.AddComponent<MeshFilter>(); //add mesh component
            MeshRenderer mr = g.AddComponent<MeshRenderer>(); //add mesh renderer component
            mr.material = material; //set material to avoid evil pinkness of missing texture
            mf.mesh.CombineMeshes(data.ToArray()); //set mesh to the combination of all of the blocks in the list
            meshes.Add(mf.mesh); //keep track of mesh so we can destroy it when it's no longer needed
            //g.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;//setting colliders takes more time. disabled for testing.
        }

        #endregion

        Debug.Log("Loaded in " + (Time.realtimeSinceStartup - startTime) + " Seconds.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Destroy(GameObject.Find("Meshys")); //destroy parent gameobject as well as children.
            foreach (Mesh m in meshes
            ) //meshes still exist even though they aren't in the scene anymore. destroy them so they don't take up memory.
                Destroy(m);
            Generate();
        }
    }

    //dunno how this works. copied it from somewhere.
    public static float Perlin3D(float x, float y, float z)
    {
        float ab = Mathf.PerlinNoise(x, y);
        float bc = Mathf.PerlinNoise(y, z);
        float ac = Mathf.PerlinNoise(x, z);

        float ba = Mathf.PerlinNoise(y, x);
        float cb = Mathf.PerlinNoise(z, y);
        float ca = Mathf.PerlinNoise(z, x);

        float abc = ab + bc + ac + ba + cb + ca;
        return abc / 6f;
    }
}