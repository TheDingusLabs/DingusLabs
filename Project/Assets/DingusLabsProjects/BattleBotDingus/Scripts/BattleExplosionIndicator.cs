using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleExplosionIndicator : MonoBehaviour
{
    public float fadeDuration = 1.1f; // Duration of the fade in seconds
    private Renderer objectRenderer;
    private Color objectColor;
    private float fadeTimer;

    void Start()
    {
        // Get the Renderer component of the GameObject
        objectRenderer = GetComponent<Renderer>();

        // Get the initial color of the object
        if (objectRenderer != null)
        {
            objectColor = objectRenderer.material.color;
        }

        fadeTimer = 0f; // Start the timer at zero

        Destroy(this.gameObject, fadeDuration);
    }

    public void SetRadius(float radius)
    {
        var multidRadius = radius * 1.3f;
        transform.localScale = new Vector3(multidRadius, multidRadius, multidRadius);
    }
    // Update is called once per frame
    void Update()
    {
        if (objectRenderer != null)
        {
            // Increment the fade timer
            fadeTimer += Time.deltaTime;

            // Calculate the new alpha value (1 means fully visible, 0 means fully transparent)
            float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);

            // Update the material color with the new alpha value
            objectRenderer.material.color = new Color(objectColor.r, objectColor.g, objectColor.b, alpha);

            // Destroy the object when it's fully transparent
            if (fadeTimer >= fadeDuration)
            {
                Destroy(gameObject);
            }
        }
    }
}
