using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseDrag : MonoBehaviour
{
    public const string DRAGGABLE_TAG = "UIDraggable";

    /// <summary>
    /// Checks whether user is already dragging a Sector
    /// </summary>
    private bool _dragging = false;

    /// <summary>
    /// The original Position of the dragged Sector
    /// </summary>
    private Vector2 _originalPosition;

    /// <summary>
    /// The original position in the Editor's Hierarchy of the dragged Sector
    /// </summary>
    private int _originalSiblingIndex;

    /// <summary>
    /// The Transform of the dragged Sector
    /// </summary>
    private Transform _objectToDrag;

    /// <summary>
    /// The glyph that represents the Dragged Sector
    /// </summary>
    private TextMeshProUGUI _objectToDragText;

    List<RaycastResult> _hitObjects = new List<RaycastResult>();

    /// <summary>
    /// Gets the first GameObject under the mouse, upon click
    /// </summary>
    GameObject GetObjectUnderMouse()
    {
        var pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        EventSystem.current.RaycastAll(pointer, _hitObjects);
        if (_hitObjects.Count <= 0) return null;

        return _hitObjects[0].gameObject;
    }

    /// <summary>
    /// Gets the Transform of the GameObject found under the mouse, upon click
    /// </summary>
    Transform GetDraggableTransformUnderMouse()
    {
        GameObject clickedObject = GetObjectUnderMouse();

        // Check that the GameObject is tagged as "draggable"
        if (clickedObject != null && clickedObject.tag == DRAGGABLE_TAG)
        {
            return clickedObject.transform;
        }

        return null;
    }

    // Update is called once per frame
    void Update()
    {
        // If AutoDefrag is on, users can't manually defrag
        if (Defragger.instance.State == DefraggerState.AUTODEFRAG) return;

        // On left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            _objectToDrag = GetDraggableTransformUnderMouse();

            // As long as there is an object to drag
            if (_objectToDrag != null)
            {
                // Start dragging it
                _dragging = true;

                // Get its original position in the Editor's Hierarchy
                _originalSiblingIndex = _objectToDrag.GetSiblingIndex();

                // Get its original position in the Scene
                _originalPosition = _objectToDrag.position;

                // Get its Glyph
                _objectToDragText = _objectToDrag.GetComponent<TextMeshProUGUI>();

                // Prevents the Sector to be TODO
                _objectToDragText.raycastTarget = false;

                // Change ther Random text in the Game Window's Footer
                Defragger.instance.FooterText.text = Defragger.instance.ChangeRandomFooterText();
            }
        }

        if (_dragging)
        {
            // Binds the dragged Sector to the cursor
            _objectToDrag.position = Input.mousePosition;
        }

        // On releasing the left mouse button
        if (Input.GetMouseButtonUp(0))
        {
            // If it was dragging an object
            if (_objectToDrag != null)
            {
                // Get the object th
                Transform objectToReplace = GetDraggableTransformUnderMouse();
                int objectToReplaceSiblingIndex;

                if (objectToReplace != null)
                {
                    objectToReplaceSiblingIndex = objectToReplace.GetSiblingIndex();

                    _objectToDrag.position = objectToReplace.position;
                    objectToReplace.position = _originalPosition;

                    _objectToDrag.SetSiblingIndex(objectToReplaceSiblingIndex);
                    objectToReplace.SetSiblingIndex(_originalSiblingIndex);

                    Defragger.instance.FooterText.text = Defragger.instance.ChangeRandomFooterText();

                    if (Defragger.instance.State != DefraggerState.FREEPAINTING)
                    {
                        Defragger.instance.ScanGrid();
                        Defragger.instance.UpdateProgressBar();
                    }

                    AudioController.instance.PlayClack();
                    AudioController.instance.PlaySeekSound();
                }
                else
                {
                    _objectToDrag.position = _originalPosition;
                }

                _objectToDragText.raycastTarget = true;
                _objectToDrag = null;
            }

            _dragging = false;
        }
    }
}