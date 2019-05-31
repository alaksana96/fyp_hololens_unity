using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.HoloFyp;

public class SceneOrganiser : MonoBehaviour {

    public static SceneOrganiser Instance;

    internal GameObject cursor;

    public GameObject label;

    internal Transform lastLabelPlaced;

    internal TextMesh lastLabelPlacedText;

    internal float probabilityThreshold = 0.8f;

    private GameObject quad;

    internal Renderer quadRenderer;

    private void Awake()
    {
        // Use this class instance as singleton
        Instance = this;

        Instance.PlaceAnalysisLabel();
    }

    /// <summary>
    /// Instantiate a Label in the appropriate location relative to the Main Camera.
    /// </summary>
    public void PlaceAnalysisLabel()
    {
        lastLabelPlaced = Instantiate(label.transform, cursor.transform.position, transform.rotation);
        lastLabelPlacedText = lastLabelPlaced.GetComponent<TextMesh>();
        lastLabelPlacedText.text = "";
        lastLabelPlaced.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        // Create a GameObject to which the texture can be applied
        quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadRenderer = quad.GetComponent<Renderer>() as Renderer;
        Material m = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
        quadRenderer.material = m;

        // Here you can set the transparency of the quad. Useful for debugging
        float transparency = 0f;
        quadRenderer.material.color = new Color(1, 1, 1, transparency);

        // Set the position and scale of the quad depending on user position
        quad.transform.parent = transform;
        quad.transform.rotation = transform.rotation;

        // The quad is positioned slightly forward in font of the user
        quad.transform.localPosition = new Vector3(0.0f, 0.0f, 3.0f);

        // The quad scale as been set with the following value following experimentation,  
        // to allow the image on the quad to be as precisely imposed to the real world as possible
        quad.transform.localScale = new Vector3(3f, 1.65f, 1f);
        quad.transform.parent = null;
    }

    public void FinaliseLabel(BoundingBoxDirection[] detections)
    {
        //For testing, get first detection
        quadRenderer = quad.GetComponent<Renderer>() as Renderer;
        Bounds quadBounds = quadRenderer.bounds;

        lastLabelPlaced.transform.parent = quad.transform;
        lastLabelPlaced.transform.localPosition = CalculateBoundingBoxPosition(quadBounds, detections[0]);
    }


    public Vector3 CalculateBoundingBoxPosition(Bounds b, BoundingBoxDirection boundingBoxDirection)
    {
        double centreX = (boundingBoxDirection.boundingBox.xmin + boundingBoxDirection.boundingBox.xmax) / 2;
        double centreY = (boundingBoxDirection.boundingBox.ymin + boundingBoxDirection.boundingBox.ymax) / 2;

        Debug.Log($"BB Centre X: {centreX}, Centre Y: {centreY}");

        double quadWidth  = b.size.normalized.x;
        double quadHeight = b.size.normalized.y;

        double normalisePos_X = (quadWidth * centreX) - (quadWidth / 2);
        double normalisePos_Y = (quadHeight * centreY) - (quadHeight / 2);

        return new Vector3((float)normalisePos_X, (float)normalisePos_Y, 0);

    }
}
