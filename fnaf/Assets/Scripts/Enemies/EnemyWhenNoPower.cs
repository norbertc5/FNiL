using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyWhenNoPower : MonoBehaviour
{
    [SerializeField] Vector3 startPosition;
    [SerializeField] Vector3 targetPosition;
    [SerializeField] AudioClip stepSound;
    [SerializeField] float speed = 1;
    float t;
    AudioSource source;
    bool jumpscareOneExecution = true;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(4);
        source = GetComponent<AudioSource>();
        StartCoroutine(PlayStepSounds());
    }

    void Update()
    {
        t += Time.deltaTime;
        transform.position = Vector3.Lerp(startPosition, targetPosition, t * speed / 10);

        // when enemy arrives to target position, he stops playing step sound and start jumpscare
        if(transform.position == targetPosition && jumpscareOneExecution)
        {
            StopAllCoroutines();
            StartCoroutine(ActiveEnemy());
            jumpscareOneExecution = false;
        }
    }

    IEnumerator PlayStepSounds()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);
            source.PlayOneShot(stepSound);
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator ActiveEnemy()
    {
        // after some time start enemy and set his nextPosition to last and enemy makes jumpscare
        yield return new WaitForSeconds(5);

        GameManager.enemies[0].gameObject.SetActive(true);
        GameManager.enemies[0].nextPosition = GameManager.enemies[0].allPossiblePositions.Length - 2;
        GameManager.enemies[0].SetPosDealyBorders(2, 4);
        StartCoroutine(GameManager.enemies[0].ChangePosition());
    }
}
