using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class PortalVfx : MonoBehaviour
{   
    public SpriteRenderer portalSprite;
    public SpriteRenderer unitSprite;
    public ParticleSystem par;
    public Light2D glow;

    public void IniPortal(Unit unit) {

        this.gameObject.SetActive(true);
        StartCoroutine(PortalEffect(unit));
    }

    public IEnumerator PortalEffect(Unit unit) {

        transform.position = unit.transform.position;

        unitSprite.sprite = unit.sprite.sprite;
        unitSprite.flipX = unit.sprite.flipX;
        unitSprite.gameObject.SetActive(true);

        float warpSpeed = 0.9f;
        float counter = unit.transform.localScale.z;

        //Begin warping unit
        while (counter > 0.20f) {
            
            counter = Mathf.Lerp(counter, 0.10f, warpSpeed * Time.deltaTime);
            unitSprite.transform.localScale = new Vector3(counter, counter, unitSprite.transform.localScale.z);
            yield return null;
        }
        unitSprite.gameObject.SetActive(false);

        float disappearSpeed = 0.55f;       
        par.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        while (portalSprite.color.a > 0f && glow.intensity > 0f) {

            Color tempColor = portalSprite.color;
            tempColor = new Color(tempColor.r, tempColor.g, tempColor.b, Mathf.Lerp(tempColor.a, -0.1f, disappearSpeed * Time.deltaTime));
            portalSprite.color = tempColor;

            glow.intensity = Mathf.Lerp(glow.intensity, -0.1f, disappearSpeed * Time.deltaTime);
            yield return null;
        }
        unitSprite.transform.localScale = Vector3.one;
        portalSprite.color = new Color(portalSprite.color.r, portalSprite.color.g, portalSprite.color.b, 1f);
        glow.intensity = 1.33f;
        this.gameObject.SetActive(false);
        yield break;
    }
}
