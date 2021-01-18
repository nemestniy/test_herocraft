using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Car Car { get; private set; }
    public float RaceTime { get; private set; }
    public int Score { get; private set; }

    [SerializeField]
    private GameObject CheckpointObject = null;

    private PathCreator pathCreator;
    private float timer = 0;

    private void Awake()
    {
        pathCreator = MapManager.Instance.PathCreator;    
    }

    void Start()
    {
        Car = DataHolder.CurrentCar;

        if(Car == null)
        {
            Debug.LogError("Car is missing.");
            return;
        }

        var path = pathCreator.GetPath();
        Car = Instantiate(Car.gameObject, path[0], Quaternion.identity).GetComponent<Car>();
        Car.Inititalize(path);

        InputManager.Instance.Initialize(Car);

        CreateCheckpoints();

        Camera.main.GetComponent<CameraController>().Initialize(Car.transform);

        Car.Finished += Car_Finished;

        MainSceneUIManager.Instance.Initialize();
    }

    private void Car_Finished()
    {
        RaceTime = timer;
        Score = Car.Score;

        var item = new DashboardItem(Car.name, RaceTime, Score);
        SaveManager.Instance.SaveResults(item);
    }

    private void Update()
    {
        timer += Time.deltaTime;
    }

    private void CreateCheckpoints()
    {
        var checkpoints = pathCreator.CalculateCheckpoints();

        foreach(var checkpoint in checkpoints)
        {
            Instantiate(CheckpointObject, checkpoint, Quaternion.identity);
        }
    }
}
