using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ShaderCode : MonoBehaviour
{

    Image image;
    Material m;
    CardVisuals visual;

    private void Awake()
    {
        image = GetComponent<Image>();
        m = new Material(image.material);
        image.material = m;
        visual = GetComponentInParent<CardVisuals>();
        

        for (int i = 0; i < image.material.enabledKeywords.Length; i++)
        {
            image.material.DisableKeyword(image.material.enabledKeywords[i]);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
        
        // SetEdition(0);
    }

    // Update is called once per frame
    void Update()
    {

        // Get the current rotation as a quaternion
        Quaternion currentRotation = transform.parent.localRotation;

        // Convert the quaternion to Euler angles
        Vector3 eulerAngles = currentRotation.eulerAngles;

        // Get the X-axis angle
        float xAngle = eulerAngles.x;
        float yAngle = eulerAngles.y;

        // Ensure the X-axis angle stays within the range of -90 to 90 degrees
        xAngle = ClampAngle(xAngle, -90f, 90f);
        yAngle = ClampAngle(yAngle, -90f, 90);


        m.SetVector("_Rotation", new Vector2(ExtensionMethods.Remap(xAngle,-20,20,-.5f,.5f), ExtensionMethods.Remap(yAngle, -20, 20, -.5f, .5f)));

    }

    // Method to clamp an angle between a minimum and maximum value
    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -180f)
            angle += 360f;
        if (angle > 180f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    /// <summary>
    /// Set the Edition for the card's shader
    /// </summary>
    /// <param name="index"></param>
    public void SetEdition(int index)
    {
        if (image == null || image.material == null) 
        {
            return;
        }
    
        // Debug.Log($"Setting edition to {Constants.CARD_EDITIONS[index]}");
    
        // First disable all edition keywords
        for (int i = 0; i < Constants.CARD_EDITIONS.Length; i++)
        {
            string keyword = "_EDITION_" + Constants.CARD_EDITIONS[i];
            if (image.material.IsKeywordEnabled(keyword))
            {
                // Debug.Log($"Disabling keyword: {keyword}");
                image.material.DisableKeyword(keyword);
            }
        }
        
        // Then enable the selected one
        string newKeyword = "_EDITION_" + Constants.CARD_EDITIONS[index];
        // Debug.Log($"Enabling keyword: {newKeyword}");
        image.material.EnableKeyword(newKeyword);
        
    
        // Force material update
        image.SetMaterialDirty();
    }
}