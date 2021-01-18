using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartUIManager : MonoBehaviour
{
    [Header("ScrollView Records")]
    public GameObject ScrollViewRecords;
    public GameObject ContentRecords;
    public GameObject DashboardItem;

    [Space]

    [Header("ScrollView Cars")]
    public GameObject ScrollViewCars;
    public GameObject ContentCars;
    public GameObject CarItem;

    private string backText = "Back";
    private string sortScore = "Sort by score";
    private string sortTime = "Sort by time";
    private bool sortByTime = true;
    private Dictionary<string, Car> carButton;

    private void Start()
    {
        if (ScrollViewRecords.activeSelf)
            ScrollViewRecords.SetActive(false);

        if (ScrollViewCars.activeSelf)
            ScrollViewCars.SetActive(false);
    }

    public void OnClickRecords(Button button)
    {
        ScrollViewRecords.SetActive(!ScrollViewRecords.activeSelf);

        if (ScrollViewRecords.activeSelf)
        {
            button.GetComponentInChildren<Text>().text = backText;
            InitializeScrollViewRecords();
        }
        else
        {
            button.GetComponentInChildren<Text>().text = button.name;
            for (int i = 0; i < ContentRecords.transform.childCount; i++)
            {
                Destroy(ContentRecords.transform.GetChild(i).gameObject);
            }
        }

    }

    public void OnClickSort(Text text)
    {
        sortByTime = !sortByTime;

        if (sortByTime)
            text.text = sortScore;
        else
            text.text = sortTime;

        for (int i = 0; i < ContentRecords.transform.childCount; i++)
        {
            Destroy(ContentRecords.transform.GetChild(i).gameObject);
        }
        InitializeScrollViewRecords();
    }

    public void OnClickPlay(Button button)
    {
        ScrollViewCars.SetActive(!ScrollViewCars.activeSelf);

        if (ScrollViewCars.activeSelf)
        {
            button.GetComponentInChildren<Text>().text = backText;
            InitializeScrollViewCars();
        }
        else
        {
            button.GetComponentInChildren<Text>().text = button.name;
            carButton.Clear();
            for (int i = 0; i < ContentCars.transform.childCount; i++)
            {
                Destroy(ContentCars.transform.GetChild(i).gameObject);
            }
        }
    }

    public void OnClickChoose(string buttonName)
    {
        if(carButton.Count == 0)
        {
            Debug.LogError("Error with car button pairs: CarButtons dictionary is empty.");
            return;
        }

        Car car = null;
        carButton.TryGetValue(buttonName, out car);

        if(car == null)
        {
            Debug.LogError("Error with car button pairs: Pair carButton is missing.");
            return;
        }

        DataHolder.CurrentCar = car;
        SceneManager.LoadScene("MainScene");
    }

    private void InitializeDashboardItem(string CarName, string Time, string Score)
    {
        var newItem = Instantiate(DashboardItem);
        newItem.transform.SetParent(ContentRecords.transform);
        var timeField = newItem.transform.Find(nameof(Time));
        var scoreField = newItem.transform.Find(nameof(Score));
        var nameField = newItem.transform.Find(nameof(CarName));

        timeField.GetComponent<Text>().text += Time;
        scoreField.GetComponent<Text>().text += Score;
        nameField.GetComponent<Text>().text += CarName;
    }

    private void InitializeScrollViewRecords()
    {
        var data = SaveManager.Instance.ReadDashboard();

        if (data == null) return;

        if (sortByTime)
            data = SortDashboardItemsByTime(data.ToArray()).ToList();
        else
            data = SortDashboardItemsByScore(data.ToArray()).ToList();

        foreach(var item in data)
        {
            InitializeDashboardItem(item.CarName, item.Time.ToString(), item.Score.ToString());
        }
    }

    private DashboardItem[] SortDashboardItemsByTime(DashboardItem[] items)
    {
        for(int i = 0; i < items.Length; i++)
        {
            for(int j = i; j < items.Length; j++)
            {
                if(items[i].Time > items[j].Time)
                {
                    var temp = items[i];
                    items[i] = items[j];
                    items[j] = temp;
                }
            }
        }

        return items;
    }

    private DashboardItem[] SortDashboardItemsByScore(DashboardItem[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            for (int j = i; j < items.Length; j++)
            {
                if (items[i].Score < items[j].Score)
                {
                    var temp = items[i];
                    items[i] = items[j];
                    items[j] = temp;
                }
            }
        }

        return items;
    }

    private void InitializeCarItem(string carName, Car car)
    {
        var carData = car.Data;
        var newItem = Instantiate(CarItem);
        newItem.transform.SetParent(ContentCars.transform);
        var text = newItem.GetComponentInChildren<Text>();
        string description = "Car name: " + carName + '\n';
        description += "MaxSpeed: " + carData.MaxSpeed + '\n';
        description += "Mass: " + carData.Mass + '\n';
        description += "Acceleration: " + carData.Acceleration + '\n';
        description += "Braking: " + carData.Braking;
        text.text = description;

        var button = newItem.GetComponentInChildren<Button>();
        button.name += car.name;
        button.onClick.AddListener(delegate { OnClickChoose(button.name); });

        carButton.Add(button.name, car);
    }

    private void InitializeScrollViewCars()
    {
        var cars = Resources.LoadAll<GameObject>("Cars");
        carButton = new Dictionary<string, Car>();
        foreach(var car in cars)
        {
            InitializeCarItem(car.name, car.GetComponent<Car>());
        }
    }
}
