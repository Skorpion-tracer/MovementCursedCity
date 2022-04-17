using System;
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
    [SerializeField] private float _timeToRotate = 2;
    [SerializeField] private float _speedWalk = 6;
    [SerializeField] private float _distanceToCollectWishes = 10;
                     
    [SerializeField] private float _viewRadius = 15;                   
    [SerializeField] private float _viewAngle = 90;                    
    [SerializeField] private LayerMask _wishMask;
    [SerializeField] private LayerMask _obstacleMask;                  
    [SerializeField] private Transform[] _waypoints;                   

    private int _currentWaypointIndex;                     

    private Vector3 _targetLastPosition = Vector3.zero;    
    private Vector3 _targetPosition;                       
    private WishCharacter _targetWish;
    private HashSet<WishCharacter> _placesToVisit;
    private HashSet<WishCharacter> _foundDesires;
    private List<WishCharacter> _placesVisited;

    private float _waitTime;                               
    private float _waitTimeToMove;                         
    private float _timeToRotateDelay;                      
    private bool _targetInRange;                           
    private bool _targetNear;                              
    private bool _isPatrol;                                
    private bool _caughtTarget;

    private void Start()
    {
        _targetPosition = Vector3.zero;
        _isPatrol = true;
        _caughtTarget = false;
        _targetInRange = false;
        _targetNear = false;
        _waitTime = _timeToCollectWishes;
        _waitTimeToMove = _delayToMove;
        _timeToRotateDelay = _timeToRotate;

        _currentWaypointIndex = 0;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _placesToVisit = new HashSet<WishCharacter>();
        _placesVisited = new List<WishCharacter>();
        _foundDesires = new HashSet<WishCharacter>();

        _navMeshAgent.isStopped = false;
        _navMeshAgent.speed = _speedWalk;
        _navMeshAgent.SetDestination(_waypoints[_currentWaypointIndex].position);
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

        _targetNear = false;
        _targetLastPosition = Vector3.zero;

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
                _isPatrol = true;
                _targetNear = false;
                Move(_speedWalk);
                _timeToRotateDelay = _timeToRotate;
                _caughtTarget = true;
                _waitTime = _timeToCollectWishes;
                _navMeshAgent.SetDestination(_waypoints[_currentWaypointIndex].position);
                _targetWish.gameObject.GetComponent<Collider>().enabled = false;
                _foundDesires.Add(_targetWish);
                NextPoint();
            }
            else
            {
                _waitTime -= Time.deltaTime;
            }
        }
    }

    private void Patroling()
    {
        if (_targetNear)
        {
            if (_timeToRotateDelay <= 0)
            {
                Move(_speedWalk);
                LookingPlayer(_targetLastPosition);
            }
            else
            {
                Stop();
                _timeToRotateDelay -= Time.deltaTime;
            }
        }
        else
        {
            _targetNear = false;
            _targetLastPosition = Vector3.zero;
            _navMeshAgent.SetDestination(_waypoints[_currentWaypointIndex].position);
            if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
            {
                if (_waitTimeToMove <= 0)
                {
                    NextPoint();
                    Move(_speedWalk);
                    _waitTimeToMove = _delayToMove;
                }
                else
                {
                    Stop();
                    _waitTimeToMove -= Time.deltaTime;
                }
            }
        }
    }

    private void NextPoint()
    {
        _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
        _navMeshAgent.SetDestination(_waypoints[_currentWaypointIndex].position);
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

    private void LookingPlayer(Vector3 target)
    {
        _navMeshAgent.SetDestination(target);
        if (Vector3.Distance(transform.position, target) <= 0.3)
        {
            if (_waitTime <= 0)
            {
                _targetNear = false;
                Move(_speedWalk);
                _navMeshAgent.SetDestination(_waypoints[_currentWaypointIndex].position);
                _waitTime = _timeToCollectWishes;
                _timeToRotateDelay = _timeToRotate;
            }
            else
            {
                Stop();
                _waitTime -= Time.deltaTime;
            }
        }
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
