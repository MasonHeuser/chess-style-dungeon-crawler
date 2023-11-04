using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public SpriteRenderer sprite;
    bool outline = false;

    public Vector2Int dir;

    void Start() {
        sprite.material.SetInt("outlineUnit", 0);
    }

    void OnDisable() {
        sprite.material.SetInt("outlineUnit", 0);
    }
    
    void OnMouseOver() {
        
        if (Control.data.contextActions && Control.data.actionAble) {

            if (Input.GetMouseButtonDown(0)) {

                Board.data.StartCoroutine(Board.data.MoveRooms(dir));
            }

            if (!outline) {
                sprite.material.SetInt("outlineUnit", 1);
                outline = true;
            }
        }
    }

    void OnMouseExit() {

        if (Control.data.contextActions && Control.data.actionAble) {
            sprite.material.SetInt("outlineUnit", 0);
            outline = false;
        }
    }
}
