using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshContorller : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NavMesh.RemoveAllNavMeshData();

        StartCoroutine(NavMeshOutOfDateCoroutine(Vector3.zero, 500.0f, true));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Coroutine to rebuild the current Scene NavMesh.
    /// </summary>
    /// <param name="playerPosition">The center of the mesh search volume</param>
    /// <param name="navigationMeshRadius">How big a volume should we search for surfaces in.</param>
    /// <param name="rebuildAll">If "true", delete any existing meshes before adding new ones.</param>
    /// <returns></returns>
    IEnumerator NavMeshOutOfDateCoroutine(Vector3 playerPosition, float navigationMeshRadius, bool rebuildAll)
    {
        // Get the list of all "sources" around us.  This is basically little gridded subsquares
        // of our terrains.
        List<NavMeshBuildSource> buildSources = new List<NavMeshBuildSource>();

        // Set up a boundary area for the build sources collector to look at;
        Bounds patchBounds = new Bounds(playerPosition,
            new Vector3(navigationMeshRadius, navigationMeshRadius, navigationMeshRadius));

        // This actually collects the potential surfaces.
        NavMeshBuilder.CollectSources(
            patchBounds,
            1 << LayerMask.NameToLayer("Terrain"),
            NavMeshCollectGeometry.PhysicsColliders,
            0,
            new List<NavMeshBuildMarkup>(),
            buildSources);

        yield return null;

        // Build some empty NavMeshData objects
        int numAgentTypes = NavMesh.GetSettingsCount();
        NavMeshData[] meshData = new NavMeshData[numAgentTypes];

        for (int agentIndex = 0; agentIndex < numAgentTypes; agentIndex++)
        {
            // Get the settings for each of our agent "sizes" (humanoid, giant humanoid)
            NavMeshBuildSettings bSettings = NavMesh.GetSettingsByIndex(agentIndex);

            // If there are any issues with the agent, print them out as a warning.
#if DEBUG
            foreach (string s in bSettings.ValidationReport(patchBounds))
            {
                Debug.LogWarning($"BuildSettings Report: {NavMesh.GetSettingsNameFromID(bSettings.agentTypeID)} : {s}");
            }
#endif
            // Make empty mesh data object.
            meshData[agentIndex] = new NavMeshData();

            AsyncOperation buildOp = NavMeshBuilder.UpdateNavMeshDataAsync(meshData[agentIndex], bSettings, buildSources, patchBounds);

            while (!buildOp.isDone) yield return null;
        }

        if (rebuildAll)
        {
            NavMesh.RemoveAllNavMeshData();
        }

        for (int nmd = 0; nmd < meshData.Length; nmd++)
        {
            NavMesh.AddNavMeshData(meshData[nmd]);
        }

        yield return null;
    }
}
