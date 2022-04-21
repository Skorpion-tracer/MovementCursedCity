using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MovePlayerAI : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private AnimationPlayer _animation;
    [SerializeField] private float _delayToMove = 0.7f;
    [SerializeField] private float _speedRun = 10;
    [SerializeField] private float _timeToCollectWishes = 2;
    [SerializeField] private float _speedWalk = 6;
    [SerializeField] private float _distanceToCollectWishes = 10;
                     
    [SerializeField] private float _viewRadius = 15;                   
    [SerializeField] private float _viewAngle = 90;                    
    [SerializeField] private LayerMask _wishMask;
    [SerializeField] private LayerMask _obstacleMask;

    private Vector3 _targetPosition;
    private WishCharacter _targetWish;
    private HashSet<WishCharacter> _placesToVisit;
    private HashSet<WishCharacter> _foundDesires;
    private List<WishCharacter> _placesVisited;

    private float _waitTime;                               
    private float _waitTimeToMove;                    
    private bool _targetInRange;
    private bool _isPatrol;                                
    private bool _caughtTarget;

    private void Start()
    {
        _targetPosition = GenericPoint(transform);
        _isPatrol = true;
        _caughtTarget = false;
        _targetInRange = false;

        _waitTime = _timeToCollectWishes;
        _waitTimeToMove = _delayToMove;

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _placesToVisit = new HashSet<WishCharacter>();
        _placesVisited = new List<WishCharacter>();
        _foundDesires = new HashSet<WishCharacter>();

        _navMeshAgent.isStopped = false;
        _navMeshAgent.speed = _speedWalk;
        _navMeshAgent.SetDestination(_targetPosition);
    }

    private void Update()
    {
        if (!_isPatrol)
        {
            Chasing();
        }
        else
        {
            EnviromentView();
            Patroling();
        }
    }

    private void Chasing()
    {
        if (_targetWish == null) return;
        if (_foundDesires.Contains(_targetWish))
        {
            _caughtTarget = true;
            return;
        }
        else
        {
            _caughtTarget = false;
        }

        if (!_caughtTarget)
        {
            Move(_speedRun);
            _navMeshAgent.SetDestination(_targetPosition);
        }
        if (Vector3.Distance(transform.position, _targetWish.transform.position) <= _distanceToCollectWishes)
        {
            Stop();

            if (_waitTime <= 0 && !_caughtTarget)
            {
                Move(_speedWalk);

                _caughtTarget = true;
                _isPatrol = true;
                _waitTime = _timeToCollectWishes;
                _navMeshAgent.SetDestination(GenericPoint(transform));
                _targetWish.gameObject.GetComponent<Collider>().enabled = false;
                _foundDesires.Add(_targetWish);
            }
            else
            {
                _waitTime -= Time.deltaTime;
            }
        }
    }

    private void Patroling()
    {
        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            if (_waitTimeToMove <= 0)
            {
                Move(_speedWalk);
                _waitTimeToMove = _delayToMove;
                _navMeshAgent.SetDestination(GenericPoint(transform));
            }
            else
            {
                Stop();
                _waitTimeToMove -= Time.deltaTime;
            }
        }
    }

    public Vector3 GenericPoint(Transform agent)
    {
        Vector3 result;

        var dis = Random.Range(10, 50);
        var randomPoint = Random.insideUnitSphere * dis;

        NavMesh.SamplePosition(agent.position + randomPoint,
            out var hit, dis, NavMesh.AllAreas);
        result = hit.position;

        return result;
    }

    private void Stop()
    {
        _navMeshAgent.isStopped = true;
        _navMeshAgent.speed = 0;
        _animation.Idle();
    }

    private void Move(float speed)
    {
        _navMeshAgent.isStopped = false;
        _navMeshAgent.speed = speed;
        _animation.Walk();
    }

    private void EnviromentView()
    {
        Collider[] targetInRange = Physics.OverlapSphere(transform.position, _viewRadius, _wishMask);

        for (int i = 0; i < targetInRange.Length; i++)
        {
            Transform target = targetInRange[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < _viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, _obstacleMask))
                {
                    _targetInRange = true;
                    _isPatrol = false;
                }
                else
                {
                    _targetInRange = false;
                }
            }
            if (_targetInRange)
            {
                if (targetInRange[i].gameObject.TryGetComponent(out IWishCharacter _))
                {
                    _targetWish = (WishCharacter)targetInRange[i].gameObject.GetComponent<IWishCharacter>();                    
                    _placesToVisit.Add(_targetWish);
                    if (_placesToVisit.Contains(_targetWish) && !_placesVisited.Contains(_targetWish))
                    {
                        _placesVisited.Add(_targetWish);
                        _targetPosition = target.transform.position;
                    }
                }                
            }
        }
    }
}
