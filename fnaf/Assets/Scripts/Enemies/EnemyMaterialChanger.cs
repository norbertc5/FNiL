using System.Collections;
using UnityEngine;

public class EnemyMaterialChanger : MonoBehaviour
{
    // this script should be attached to enemy model standing in front of security room door
    // make that enemy material changes animationToPlay bit. Thanks to it enemy isn'timeToChangeState visible when he's standing in fron of door
    // make also that light blinks when enemy appears in front of door

    [Header("Main")]
    [Space(6)]

    [SerializeField] Material enemyMaterial;
    [SerializeField] EnemiesBehaviour enemiesBehaviourScript;
    [SerializeField] bool makeMainLightBlinkRightAway;  // can main light blinking without corridor light
    bool hasBeenLighted;  // has player turned light on when enemy was standing in front of security room door

    [Space(10)]
    [Header("Other")]
    [Space(6)]

    [SerializeField] Material additionalMaterial;  // when enemy has more than 1 material

    private void OnEnable()
    {
        SetMaterialToDark();

        // when player is using corridor light and enemy appeard over security room door invoke LightCorridorBlink
        // batteryState must be greater than 0 because light can'timeToChangeState blink when power ran out
        if (enemiesBehaviourScript.lightCorridorScript.isOn && Battery.batteryState > 0)
            StartCoroutine(enemiesBehaviourScript.lightCorridorScript.LightCorridorBlink());
    }

    private void OnDisable()
    {
        // make that enemy stops standing in front of security room door, he can make blinking light again next time
        hasBeenLighted = false;
        SetMaterialToLit();
    }

    void Update()
    {
        if (Battery.batteryState > 0 && ((enemiesBehaviourScript.lightCorridorScript.isOn && !makeMainLightBlinkRightAway) || makeMainLightBlinkRightAway))
        {
            SetMaterialToLit();

            // light in security room can'timeToChangeState blink when player turns on light and door is closed
            if (!hasBeenLighted && !enemiesBehaviourScript.doorScript.isOn)
            {
                StartCoroutine(GameManager.MianLightBlinking());
                hasBeenLighted = true;
            }
        }
        else
        {
            SetMaterialToDark();
        }
    }

    void SetMaterialToLit()
    {
        // make enemy material visible in darkness
        // deafult set for enemyMaterial
        enemyMaterial.SetFloat("_Metallic", 0);
        enemyMaterial.SetFloat("_Glossiness", 0.5f);

        if(additionalMaterial != null)
            additionalMaterial.SetFloat("_Metallic", 0);
    }

    void SetMaterialToDark()
    {
        // make enemy invisible in darkness
        // using when enemy is standing in front of security room door (in this case he shouldn'timeToChangeState be visible)
        enemyMaterial.SetFloat("_Metallic", 1);
        enemyMaterial.SetFloat("_Glossiness", 0);

        if (additionalMaterial != null)
            additionalMaterial.SetFloat("_Metallic", 1);
    }
}
