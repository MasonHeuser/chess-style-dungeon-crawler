using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DamageTextProperties : MonoBehaviour
{
    float disappearTimer;
    public TextMeshPro text;
    public Rigidbody2D body;

    // Start is called before the first frame update
    void OnEnable()
    {
        body.velocity = Vector3.zero;
        float direction = Random.Range(-65f, 65f);
        //This way you can go right and left
        float force = Random.Range(180f, 220f); 
        //This way you can variate the force
        body.AddForce(new Vector2(direction, force));

        // Text text = GetComponent<Text>();
        var tempColor = text.color;
        tempColor.a = 1f;
        text.color = tempColor;

        disappearTimer = Random.Range(0.75f, 1.25f);
    }

    void Update() 
    {   
        if (this.gameObject.activeSelf) {

            disappearTimer -= Time.deltaTime;
            if (disappearTimer <= 0f) {
                float disappearSpeed = 0.75f;

                // Text text = GetComponent<Text>();
                var tempColor = text.color;
                tempColor.a -= disappearSpeed * Time.deltaTime;
                text.color = tempColor;

                if (text.color.a <= 0f) {
                    this.gameObject.SetActive(false);
                }
            }
        }
    }
}
