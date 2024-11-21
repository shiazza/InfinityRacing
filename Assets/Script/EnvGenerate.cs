using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class EnvGenerate : MonoBehaviour
{
    public static EnvGenerate instance = null;

    [SerializeField] private SpriteShapeController _spriteShapeController;
    [SerializeField] private Transform playerTransform; // Referensi ke mobil/player
    [SerializeField] private bool automaticallyStart = true;
    [SerializeField] private bool isEndless = true;
    
    [Header("Level Settings")]
    [SerializeField, Range(3f, 100f)] private int _levelLength = 50;
    [SerializeField, Range(1f, 50f)] private float _xMultiplier = 2f;
    [SerializeField, Range(1f, 50f)] private float _yMultiplier = 2f;
    [SerializeField, Range(0f, 1f)] private float _curveSmoothness = 0.5f;
    [SerializeField] private float _noiseStep = 0.5f;
    [SerializeField] private float _bottom = 10f;
    [SerializeField] private float generationThreshold = 10f; // Jarak threshold untuk generate terrain baru
    
    private Vector3 _lastPos;
    private bool stopMoving = false;
    private List<Vector3> activePoints = new List<Vector3>();
    private float _lastGeneratedX;
    private int _startIndex = 0;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        if (!Application.isPlaying) return;
        
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        GenerateInitialTerrain();
        _lastGeneratedX = _lastPos.x;

        if (automaticallyStart)
            StartCoroutine(TrackPlayerPosition());
    }

    private IEnumerator TrackPlayerPosition()
    {
        while (isEndless || activePoints.Count > 0)
        {
            if (stopMoving)
            {
                yield return null;
                continue;
            }

            // Check jarak player dari terrain terakhir
            if (playerTransform.position.x > _lastGeneratedX - generationThreshold)
            {
                GenerateForwardTerrain();
            }

            // Check untuk menghapus terrain yang sudah dilewati
            if (playerTransform.position.x > transform.position.x + (_startIndex + 10) * _xMultiplier)
            {
                RemovePassedTerrain();
            }

            yield return new WaitForSeconds(0.1f); // Check setiap 0.1 detik untuk optimasi
        }
    }

    private void GenerateInitialTerrain()
    {
        _spriteShapeController.spline.Clear();
        activePoints.Clear();

        for (int i = 0; i < _levelLength; i++)
        {
            Vector3 newPoint = transform.position + new Vector3(i * _xMultiplier, 
                Mathf.PerlinNoise(0, i * _noiseStep) * _yMultiplier);
            
            _spriteShapeController.spline.InsertPointAt(i, newPoint);
            activePoints.Add(newPoint);

            if (i != 0 && i != _levelLength - 1)
            {
                _spriteShapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
                _spriteShapeController.spline.SetLeftTangent(i, Vector3.left * _xMultiplier * _curveSmoothness);
                _spriteShapeController.spline.SetRightTangent(i, Vector3.right * _xMultiplier * _curveSmoothness);
            }
        }

        // Add bottom points
        _lastPos = activePoints[activePoints.Count - 1];
        _spriteShapeController.spline.InsertPointAt(_levelLength, new Vector3(_lastPos.x, transform.position.y - _bottom));
        _spriteShapeController.spline.InsertPointAt(_levelLength + 1, new Vector3(transform.position.x, transform.position.y - _bottom));

        _lastGeneratedX = _lastPos.x;
    }

    private void GenerateForwardTerrain()
    {
        // Generate 4 new points
        int currentIndex = _spriteShapeController.spline.GetPointCount() - 2; // -2 for bottom points
        
        for (int i = 0; i < 4; i++)
        {
            Vector3 newPoint = new Vector3(
                _lastPos.x + (i + 1) * _xMultiplier,
                transform.position.y + Mathf.PerlinNoise(_startIndex * _noiseStep, (currentIndex + i) * _noiseStep) * _yMultiplier
            );

            _spriteShapeController.spline.InsertPointAt(currentIndex + i, newPoint);
            activePoints.Add(newPoint);

            _spriteShapeController.spline.SetTangentMode(currentIndex + i, ShapeTangentMode.Continuous);
            _spriteShapeController.spline.SetLeftTangent(currentIndex + i, Vector3.left * _xMultiplier * _curveSmoothness);
            _spriteShapeController.spline.SetRightTangent(currentIndex + i, Vector3.right * _xMultiplier * _curveSmoothness);
        }

        _lastPos = activePoints[activePoints.Count - 1];
        _lastGeneratedX = _lastPos.x;
        
        // Update bottom points
        int lastIndex = _spriteShapeController.spline.GetPointCount() - 2;
        _spriteShapeController.spline.SetPosition(lastIndex, new Vector3(_lastPos.x, transform.position.y - _bottom));
        _spriteShapeController.spline.SetPosition(lastIndex + 1, new Vector3(transform.position.x + _startIndex * _xMultiplier, transform.position.y - _bottom));
    }

    private void RemovePassedTerrain()
    {
        // Remove points that are behind
        int pointsToRemove = 4;
        for (int i = 0; i < pointsToRemove; i++)
        {
            if (_spriteShapeController.spline.GetPointCount() > _levelLength)
            {
                _spriteShapeController.spline.RemovePointAt(0);
                if (activePoints.Count > 0)
                {
                    activePoints.RemoveAt(0);
                }
                _startIndex++;
            }
        }

        // Update bottom point
        int lastIndex = _spriteShapeController.spline.GetPointCount() - 1;
        _spriteShapeController.spline.SetPosition(lastIndex, new Vector3(transform.position.x + _startIndex * _xMultiplier, transform.position.y - _bottom));
    }

    public void StopGeneration()
    {
        stopMoving = true;
    }

    public void ResumeGeneration()
    {
        stopMoving = false;
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        GenerateInitialTerrain();
    }
}