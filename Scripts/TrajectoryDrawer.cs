using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryDrawer : MonoBehaviour
{
    [Header("Line Renderer")]
    [SerializeField] private LineRenderer _lineRenderer;

    [Header("Trajectory Settings")]
    [SerializeField] private int _resolution = 30; // 궤적 점의 개수
    [SerializeField] private float _timeStep = 0.1f; // 시간 간격

    private Vector2 _gravity;

    void Start()
    {
        _gravity = Physics2D.gravity; // 현재 물리 환경의 중력 가져오기
    }

    public void DrawTrajectory(Vector2 startPosition, Vector2 velocity)
    {
        Vector3[] points = new Vector3[_resolution];

        for (int i = 0; i < _resolution; i++)
        {
            float t = i * _timeStep; // 현재 시간

            // 물리 공식을 사용하여 궤적 점 계산
            float x = startPosition.x + velocity.x * t;
            float y = startPosition.y + velocity.y * t + 0.5f * _gravity.y * t * t;

            points[i] = new Vector3(x, y, 0);
        }

        _lineRenderer.positionCount = points.Length;
        _lineRenderer.SetPositions(points);
    }

    public void ClearTrajectory()
    {
        _lineRenderer.positionCount = 0;
    }
}