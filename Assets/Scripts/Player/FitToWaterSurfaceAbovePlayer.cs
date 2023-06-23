using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FitToWaterSurfaceAbovePlayer : MonoBehaviour
{
    [SerializeField] private WaterSurface targetSurface;
    [SerializeField] private Transform player;

    // Internal search params
    private WaterSearchParameters searchParameters;
    private WaterSearchResult searchResult;
    private bool isTargetSurfaceNull;


    // Update is called once per frame
    private void Start()
    {
        isTargetSurfaceNull = targetSurface == null;
    }

    private void Update()
    {
        if (isTargetSurfaceNull) return;
        var transform1 = transform;
        transform1.position = player.position; // start at player position
            
        // Build the search parameters
        searchParameters.startPositionWS = searchResult.candidateLocationWS;
        searchParameters.targetPositionWS = transform1.position;
        searchParameters.error = 0.01f;
        searchParameters.maxIterations = 8;

        // Do the search
        if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
        {
            gameObject.transform.position = searchResult.projectedPositionWS;
        }
    }
}