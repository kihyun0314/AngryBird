using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlingShotHandler : MonoBehaviour
{
    [Header("Line Renderer")]
    [SerializeField] private LineRenderer _leftLineRenderer;
    [SerializeField] private LineRenderer _rightLineRenderer;

    [Header("Transform References")]
    [SerializeField] private Transform _leftStartPosition;
    [SerializeField] private Transform _rightStartPosition;
    [SerializeField] private Transform _centerPosition;
    [SerializeField] private Transform _idlePosition;
    
    [Header("SlingShot Stats")]
    [SerializeField] private float _maxDistance = 3.5f;
    [SerializeField] private float _shotForce = 5f;
    [SerializeField] private float _timeBetweenBirdRespawns = 2f;
    
    [Header("Scripts")]
    [SerializeField] private SlingShotArea _slingShotArea;
    [SerializeField] private CameraManager _cameraManager;
    
    [Header("Bird")]
    [SerializeField] private AngryBird _angryBirdPrefab;
    [SerializeField] private float _angryBirdPositionOffset = 2f;
    
    [Header("Sounds")]
    [SerializeField] private AudioClip _elasticPulledClip;
    [SerializeField] private AudioClip _elasticReleasedClip;
    
    [Header("Trajectory")]
    [SerializeField] private TrajectoryDrawer _trajectoryDrawer;

    private Vector2 _slingShotLinesPosition;
    
    private Vector2 _direction;
    private Vector2 _directionNormalized;

    private bool _clickedWithinArea;
    private bool _birdOnSlingshot;

    private AngryBird _spanwedAngryBird;
    
    private AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        
        _leftLineRenderer.enabled = false;
        _rightLineRenderer.enabled = false;

        SpawnAngryBird();
    }
    
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && _slingShotArea.IsWithinSlingshotArea())
        {
            _clickedWithinArea = true;

            if (_birdOnSlingshot)
            {
                SoundManager.instance.PlayClip(_elasticPulledClip, _audioSource);
                _cameraManager.SwitchToFollowCam(_spanwedAngryBird.transform);
            }
        }
        
        if (Mouse.current.leftButton.isPressed && _clickedWithinArea && _birdOnSlingshot)
        {
            DrawSlingShot();
            PositionAndRotateAngryBird();
            
            Vector2 birdVelocity = _direction * _shotForce / _spanwedAngryBird.GetComponent<Rigidbody2D>().mass;
            _trajectoryDrawer.DrawTrajectory(_spanwedAngryBird.transform.position, birdVelocity);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && _birdOnSlingshot)
        {
            _trajectoryDrawer.ClearTrajectory();
            
            if (GameManager.instance.HasEnoughShots())
            {
                _clickedWithinArea = false;
                _birdOnSlingshot = false;

                _spanwedAngryBird.LaunchBird(_direction, _shotForce);
                
                SoundManager.instance.PlayClip(_elasticReleasedClip, _audioSource);
                
                GameManager.instance.UseShot();
                SetLines(_centerPosition.position);

                if (GameManager.instance.HasEnoughShots())
                {
                    StartCoroutine(SpawnAngryBirdAfterTime());
                }
            }
        }
    }
    
    #region Slingshot Methods

    void DrawSlingShot()
    {
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        
        _slingShotLinesPosition = _centerPosition.position + Vector3.ClampMagnitude(touchPosition - _centerPosition.position, _maxDistance);
        
        SetLines(_slingShotLinesPosition);
        
        _direction = (Vector2)_centerPosition.position - _slingShotLinesPosition;
        _directionNormalized = _direction.normalized;
    }

    void SetLines(Vector2 position)
    {
        if (!_leftLineRenderer.enabled && !_rightLineRenderer.enabled)
        {
            _leftLineRenderer.enabled = true;
            _rightLineRenderer.enabled = true;
        }
        
        _leftLineRenderer.SetPosition(0, position);
        _leftLineRenderer.SetPosition(1, _leftStartPosition.position);
        
        _rightLineRenderer.SetPosition(0, position);
        _rightLineRenderer.SetPosition(1, _rightStartPosition.position);
    }
    
    #endregion
    
    #region Angry Bird Methods

    void SpawnAngryBird()
    {
        SetLines(_idlePosition.position);
        
        Vector2 dir = (_centerPosition.position - _idlePosition.position).normalized;
        Vector2 spawnPosition = (Vector2)_idlePosition.position + dir * _angryBirdPositionOffset;
        
        _spanwedAngryBird = Instantiate(_angryBirdPrefab, spawnPosition, Quaternion.identity);
        _spanwedAngryBird.transform.right = dir;
        
        _birdOnSlingshot = true;
    }

    private void PositionAndRotateAngryBird()
    {
        _spanwedAngryBird.transform.position = _slingShotLinesPosition + _directionNormalized * _angryBirdPositionOffset;
        _spanwedAngryBird.transform.right = _directionNormalized;
    }

    private IEnumerator SpawnAngryBirdAfterTime()
    {
        yield return new WaitForSeconds(_timeBetweenBirdRespawns);

        SpawnAngryBird();
        
        _cameraManager.SwitchToIdleCam();
    }
    
    #endregion
}