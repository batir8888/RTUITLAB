using System.Collections;
using UnityEngine;

public class DiscoBall : MonoBehaviour
{
    private WaitForSeconds _timer;
    
    private void Start()
    {
        _timer = new WaitForSeconds(0.5f);
        StartCoroutine(RandomRotation());
    }

    private IEnumerator RandomRotation()
    {
        while (true)
        {
            var targetRotation = new Vector3(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetRotation), 0.25f);
            yield return _timer;
        }
    }
}
