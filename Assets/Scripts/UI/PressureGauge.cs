using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PressureGauge : MonoBehaviour
{
    [SerializeField] private TankController tankController;
    
    [SerializeField] private GameObject needle;
    [SerializeField] private GameObject labelTemplate;
    [SerializeField] private int numLabels;

    private Vector3 startingRotation; // the rotation of the needle at the 0 o'clock position
    private float maxPressureAngle; // angle at 100% pressure, depends on startingRotation
    private float zeroPressureAngle; // angle at 0% pressure, depends on startingRotation
    private float totalAngleRange;


    private void Awake()
    {
        labelTemplate.SetActive(false);
    }

    private void Start()
    {
        var localEulerAngles = needle.transform.localEulerAngles;
        // rotation is off in the prefab?
        startingRotation = new Vector3(localEulerAngles.x, localEulerAngles.y - 90, localEulerAngles.z + 90);
        
        maxPressureAngle = startingRotation.y + 120f;
        zeroPressureAngle = startingRotation.y - 120f;
        totalAngleRange = maxPressureAngle - zeroPressureAngle;
        
        CreateLabels();
    }

    private void Update()
    {
        needle.transform.localEulerAngles = new Vector3(GetPressureRotation(), startingRotation.y, startingRotation.z);
    }

    private void CreateLabels()
    {
        for (int i = 0; i <= numLabels; i++)
        {
            GameObject label = Instantiate(labelTemplate, transform);
            float normalisedLabelPressure = (float)i / numLabels; // 0 to 1 value
            var labelAngle = zeroPressureAngle + normalisedLabelPressure * totalAngleRange; // how much the current label should be rotated from zero angle

            var angles = label.transform.localEulerAngles;
            label.transform.localEulerAngles = new Vector3(labelAngle - 90, angles.y - 90, angles.z + 90);

            // gross hardcoded value NOTE: LABEL MUST BE CALLED "Label" in template
            TextMeshPro labelText = label.transform.Find("Label").GetComponent<TextMeshPro>();
            labelText.text = Mathf.RoundToInt(normalisedLabelPressure * tankController.MaxTankPressure).ToString();
            var textAngles = labelText.transform.localEulerAngles;

            labelText.transform.localEulerAngles = new Vector3(textAngles.x , textAngles.y + labelAngle, textAngles.z); // un-rotate labels
            label.SetActive(true);
        }
        
    }
    
    private float GetPressureRotation()
    {
        float normalisedPressure = tankController.CurrentTankPressure / tankController.MaxTankPressure; // normalise so gives a value between 0 and 1
        return zeroPressureAngle + normalisedPressure * totalAngleRange;
    }

}
