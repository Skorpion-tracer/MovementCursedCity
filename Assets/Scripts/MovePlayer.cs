using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum TypeMove
{
    WASD,
    Click
}

[RequireComponent(typeof(NavMeshAgent))]
public class MovePlayer : MonoBehaviour
{
    [SerializeField] private TypeMove _typeMove = TypeMove.Click;
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private float _speed = 5;

    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
        _navMeshAgent = GetComponent<NavMeshAgent>();

        _navMeshAgent.speed = _speed;
    }

    private void Update()
    {
        switch (_typeMove)
        {
            case TypeMove.WASD:
                MoveWASD();
                break;
            case TypeMove.Click:
                MoveClick();
                break;
        }
    }

    private void MoveWASD()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        transform.Translate(Vector3.forward * vertical * _speed * Time.deltaTime);
        transform.Translate(Vector3.right * horizontal * _speed * Time.deltaTime);
    }

    private void MoveClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit[] hits = new RaycastHit[1];
            Physics.RaycastNonAlloc(_camera.ScreenPointToRay(Input.mousePosition), hits, float.MaxValue);
            if (hits.Length == 0)
            {
                return;
            }

            if (hits[0].collider.TryGetComponent<Ground>(out _))
            {
                _navMeshAgent.destination = hits[0].point;

                Debug.Log(hits[0].point);
            }
        }
    }
}
