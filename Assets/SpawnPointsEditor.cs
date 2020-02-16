using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnPoints), true)]
public class SpawnPointsEditor : Editor
{
    int controlId;

    SpawnPoints spawnPoints;

    const float spawnPointRadius = 1f;

    int hoveredSpawnPointId = -1;
    int draggedSpawnPointId = -1;

    void OnEnable()
    {
        Tools.hidden = true;
        spawnPoints = target as SpawnPoints;
        controlId = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
    }

    void OnDisable()
    {
        Tools.hidden = false;
    }

    public void OnSceneGUI ()
    {
        Tools.current = Tool.None;

        Event guiEvent = Event.current;

        if(Event.current.type == EventType.Repaint)
        {
            Draw();
        }
        else if(Event.current.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(controlId);
        }
        else
        {
            HandleInput(guiEvent);
        }
    }

    void Draw()
    {
        HandleUtility.Repaint();

        DrawHoveredInformation();

        for (int i = 0; i < spawnPoints.spawnPositions.Count; i++)
        {
            Handles.color = Color.yellow;

            if(hoveredSpawnPointId == i)
            {
                Handles.color = Color.blue;
            }

            Handles.SphereHandleCap(controlId, spawnPoints.spawnPositions[i], Quaternion.identity, spawnPointRadius, EventType.Repaint);
        }
    }

    void DrawHoveredInformation()
    {
        if(hoveredSpawnPointId == -1) return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.green;
 
        Handles.BeginGUI();
        Vector3 pos = spawnPoints.spawnPositions[hoveredSpawnPointId];
        Vector2 pos2D = HandleUtility.WorldToGUIPoint(pos);
        GUI.Label(new Rect(pos2D.x + 30, pos2D.y, 100, 100), $"Spawn point {hoveredSpawnPointId}\n{spawnPoints.spawnPositions[hoveredSpawnPointId].ToString()}", style);
        Handles.EndGUI();
    }

    private void HandleInput(Event guiEvent)
    {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);

        if (Physics.Raycast(mouseRay, out RaycastHit hit, Mathf.Infinity, spawnPoints.raycastLayerMask))
        {
            Vector3 mousePosition = hit.point;

            HandleSpawnPointHovering(mousePosition);

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Control)
            {
                HandleControlLeftMouseDown(mousePosition);
            }
            else if(guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
            {
                HandleLeftMouseDown(mousePosition);
            }
            else if(guiEvent.type == EventType.MouseDrag && guiEvent.button == 0)
            {
                HandleLeftMouseDrag(mousePosition);
            }
            else if(guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
            {
                HandleRightMouseDown(mousePosition);
            }
        }

        if(guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
        {
            HandleLeftMouseUp();
        }
    }

    void HandleControlLeftMouseDown(Vector3 mousePosition)
    {
        spawnPoints.spawnPositions.Add(mousePosition);
        Repaint();
    }

    void HandleLeftMouseDown(Vector3 mousePosition)
    {
        if(hoveredSpawnPointId == -1) return;

        draggedSpawnPointId = hoveredSpawnPointId;
    }


    void HandleLeftMouseDrag(Vector3 mousePosition)
    {
        if(draggedSpawnPointId == -1) return;

        spawnPoints.spawnPositions[draggedSpawnPointId] = mousePosition;

        Repaint();
    }

    void HandleLeftMouseUp()
    {
        draggedSpawnPointId = -1;
    }

    void HandleRightMouseDown(Vector3 mousePosition)
    {
        if(hoveredSpawnPointId == -1) return;

        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Delete Spawn point"), false, DeleteSpawnPoint, 1);
        menu.ShowAsContext();

        Repaint();
    }

    void HandleSpawnPointHovering(Vector3 mousePosition)
    {
        hoveredSpawnPointId = GetHoveredSpawnPoint(mousePosition);

        Repaint();
    }

    int GetHoveredSpawnPoint(Vector3 mousePosition)
    {
        for (int i = 0; i < spawnPoints.spawnPositions.Count; i++)
        {
            if(Vector3.Distance(mousePosition, spawnPoints.spawnPositions[i]) <= spawnPointRadius)
            {
                return i;
            }
        }

        return -1;
    }

    void DeleteSpawnPoint(object obj)
    {
        if(hoveredSpawnPointId == -1) return;

        spawnPoints.spawnPositions.RemoveAt(hoveredSpawnPointId);

        hoveredSpawnPointId = -1;
        draggedSpawnPointId = -1;

        Repaint();
    }
}
