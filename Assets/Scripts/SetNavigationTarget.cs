using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class SetNavigationTarget : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown navigationTargetDropdown;
    
    [SerializeField]
    private List<Target> navigationTargetObjects = new List<Target>();
    
    private NavMeshPath path; // current calculated path
    private LineRenderer line; // component to display path
    private Vector3 targetPosition = Vector3.zero;
    private bool lineToggle = false;
    
    [SerializeField]
    public float lineHeight = 0.5f; // Height above ground for the line, adjustable in inspector
    
    private void Start()
    {
        path = new NavMeshPath();
        line = transform.GetComponent<LineRenderer>();
        
        // Check if LineRenderer exists
        if (line == null) {
            Debug.LogError("XYZ123 No LineRenderer component found on this GameObject!");
            return;
        }
        
        line.enabled = true;
        Debug.Log("XYZ123 SetNavigationTarget initialized. Line visibility: " + lineToggle);
    }
    
    private void Update()
    {
        if (lineToggle && targetPosition != Vector3.zero){
            bool pathFound = NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);
            
            if (!pathFound) {
                Debug.LogWarning("XYZ123 No path found to target position!");
                return;
            }
            
            if (path.corners.Length <= 1) {
                Debug.Log("XYZ123 Path only has " + path.corners.Length + " corners. May not be visible.");
            }
            // Create elevated path points
            Vector3[] elevatedCorners = new Vector3[path.corners.Length];
            for (int i = 0; i < path.corners.Length; i++) {
                elevatedCorners[i] = path.corners[i] + new Vector3(0, lineHeight, 0); // lineHeight meters above ground
            }
            
            
            line.positionCount = path.corners.Length;
            line.SetPositions(path.corners);
            
            // Debug path points
            for (int i = 0; i < path.corners.Length; i++) {
                Debug.Log("XYZ123 Path point " + i + ": " + path.corners[i]);
            }
        }
    }
    
    public void SetCurrentNavigationTarget(int selectedValue) {
        if (navigationTargetDropdown.options.Count <= selectedValue) {
            Debug.LogError("XYZ123 Selected dropdown value out of range!");
            return;
        }
        
        targetPosition = Vector3.zero;
        string selectedText = navigationTargetDropdown.options[selectedValue].text;
        Debug.Log("XYZ123 Setting navigation target to: " + selectedText);
        
        Target currentTarget = navigationTargetObjects.Find(x => x.Name.Equals(selectedText));
        if(currentTarget != null) {
            targetPosition = currentTarget.PositionObject.transform.position;
            Debug.Log("XYZ123 Target position set to: " + targetPosition);
        } else {
            Debug.LogError("XYZ123 Target not found in navigationTargetObjects list: " + selectedText);
        }

        // Check if positions are on NavMesh
        NavMeshHit hit;
        bool sourceOnNavMesh = NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas);
        bool targetOnNavMesh = NavMesh.SamplePosition(targetPosition, out hit, 2.0f, NavMesh.AllAreas);
        Debug.Log($"XYZ123 Source on NavMesh: {sourceOnNavMesh}, Target on NavMesh: {targetOnNavMesh}");

        // If target isn't on NavMesh, find nearest valid point
        if (!targetOnNavMesh && NavMesh.SamplePosition(targetPosition, out hit, 5.0f, NavMesh.AllAreas)) {
            targetPosition = hit.position;
            Debug.Log("XYZ123 Adjusted target position to nearest NavMesh point: " + targetPosition);
        }

        if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas)) {
            // Use hit.position as the source position for path calculation
            bool pathSuccess = NavMesh.CalculatePath(hit.position, targetPosition, NavMesh.AllAreas, path);
            Debug.Log("Path calculation using adjusted source: " + pathSuccess);
            
            if (pathSuccess) {
                line.positionCount = path.corners.Length;
                line.SetPositions(path.corners);
                lineToggle = true;
                line.enabled = true;
            }
        } else {
            Debug.LogError("Cannot find any NavMesh near source position!");
        }
    }
    
    public void ToggleVisibility() {
        lineToggle = !lineToggle;
        line.enabled = lineToggle;
        Debug.Log("XYZ123 Path visibility toggled: " + lineToggle);
        
        // // Force immediate path calculation if toggling on
        // if (lineToggle && targetPosition != Vector3.zero) {
        //     NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);
        //     line.positionCount = path.corners.Length;
        //     line.SetPositions(path.corners);
        // }
    }
}