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

    //public Vector3 CalculateBoundingBoxPosition(Bounds b, BoundingBox boundingBox)
    //{
    //    Debug.Log($"BB: left {boundingBox.left}, top {boundingBox.top}, width {boundingBox.width}, height {boundingBox.height}");

    //    double centerFromLeft = boundingBox.left + (boundingBox.width / 2);
    //    double centerFromTop = boundingBox.top + (boundingBox.height / 2);
    //    Debug.Log($"BB CenterFromLeft {centerFromLeft}, CenterFromTop {centerFromTop}");

    //    double quadWidth = b.size.normalized.x;
    //    double quadHeight = b.size.normalized.y;
    //    Debug.Log($"Quad Width {b.size.normalized.x}, Quad Height {b.size.normalized.y}");

    //    double normalisedPos_X = (quadWidth * centerFromLeft) - (quadWidth / 2);
    //    double normalisedPos_Y = (quadHeight * centerFromTop) - (quadHeight / 2);

    //    return new Vector3((float)normalisedPos_X, (float)normalisedPos_Y, 0);
    //}

    /// <summary>
    /// Set the Tags as Text of the last label created. 
    /// </summary>
    //public void FinaliseLabel(AnalysisRootObject analysisObject)
    //{
    //    if (analysisObject.predictions != null)
    //    {
    //        lastLabelPlacedText = lastLabelPlaced.GetComponent<TextMesh>();
    //        // Sort the predictions to locate the highest one
    //        List<Prediction> sortedPredictions = new List<Prediction>();
    //        sortedPredictions = analysisObject.predictions.OrderBy(p => p.probability).ToList();
    //        Prediction bestPrediction = new Prediction();
    //        bestPrediction = sortedPredictions[sortedPredictions.Count - 1];

    //        if (bestPrediction.probability > probabilityThreshold)
    //        {
    //            quadRenderer = quad.GetComponent<Renderer>() as Renderer;
    //            Bounds quadBounds = quadRenderer.bounds;

    //            // Position the label as close as possible to the Bounding Box of the prediction 
    //            // At this point it will not consider depth
    //            lastLabelPlaced.transform.parent = quad.transform;
    //            lastLabelPlaced.transform.localPosition = CalculateBoundingBoxPosition(quadBounds, bestPrediction.boundingBox);

    //            // Set the tag text
    //            lastLabelPlacedText.text = bestPrediction.tagName;

    //            // Cast a ray from the user's head to the currently placed label, it should hit the object detected by the Service.
    //            // At that point it will reposition the label where the ray HL sensor collides with the object,
    //            // (using the HL spatial tracking)
    //            Debug.Log("Repositioning Label");
    //            Vector3 headPosition = Camera.main.transform.position;
    //            RaycastHit objHitInfo;
    //            Vector3 objDirection = lastLabelPlaced.position;
    //            if (Physics.Raycast(headPosition, objDirection, out objHitInfo, 30.0f, SpatialMapping.PhysicsRaycastMask))
    //            {
    //                lastLabelPlaced.position = objHitInfo.point;
    //            }
    //        }
    //    }
    //    // Reset the color of the cursor
    //    cursor.GetComponent<Renderer>().material.color = Color.green;

    //    // Stop the analysis process
    //    ImageCapture.Instance.ResetImageCapture();
    //}

    /// <summary>
    /// This method hosts a series of calculations to determine the position 
    /// of the Bounding Box on the quad created in the real world
    /// by using the Bounding Box received back alongside the Best Prediction
    /// </summary>
    //public Vector3 CalculateBoundingBoxPosition(Bounds b, BoundingBox boundingBox)
    //{
    //    Debug.Log($"BB: left {boundingBox.left}, top {boundingBox.top}, width {boundingBox.width}, height {boundingBox.height}");

    //    double centerFromLeft = boundingBox.left + (boundingBox.width / 2);
    //    double centerFromTop = boundingBox.top + (boundingBox.height / 2);
    //    Debug.Log($"BB CenterFromLeft {centerFromLeft}, CenterFromTop {centerFromTop}");

    //    double quadWidth = b.size.normalized.x;
    //    double quadHeight = b.size.normalized.y;
    //    Debug.Log($"Quad Width {b.size.normalized.x}, Quad Height {b.size.normalized.y}");

    //    double normalisedPos_X = (quadWidth * centerFromLeft) - (quadWidth / 2);
    //    double normalisedPos_Y = (quadHeight * centerFromTop) - (quadHeight / 2);

    //    return new Vector3((float)normalisedPos_X, (float)normalisedPos_Y, 0);
    //}
}
