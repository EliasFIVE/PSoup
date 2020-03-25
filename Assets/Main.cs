using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    static public Main S; //Одиночка, хранит данные об общем статусе и функции начальной генерации мира

    [Header("Aquarium parameters")]
    public GameObject brickPrefab;
    public float height;
    public float width;
    public float wallThickness;

    [Header("World rules options")]
    public GameObject cellPrefab;
    public Color defaultColor;
    public float startEnergy;

    [Header("Сommon properties of all Cells")]
    public float birthDistAddition = 0.25f; //!!!! Расстояние от центра родителя до рождающихся клеток в долях от размера родителя
    public float birthDistIndependence = 0.63f; //Расстояние от центра родителя до отмектки, где клетка становится независимой, в долях от размера родителя
    public float dyingTime = 2f;//Продолжительность процесса смерти
    public float borningTime = 2f; // Время, за которое происходит рождение клетки
    public float deadSizeRatio = 0.5f;          //Определяет отношение размера мертвой клетки к размуру живой, влияет на massToSize
    public float energyInitial = 0.25f;         //Начальная энергия клетки в долях от родительской
    public float fertileEnergyCost = 2f;     //Цена энергии на деление в долях от энергии рожденной клетки
    public float energyToMass = 1f;  //Отношение эенергии к массе: mass = energyStoke*energyToMass

    [Header("World statistic")]
    public int cellWasAtAll = 0;
    public int cellAlive = 0;
    public int cellDied = 0;
    public List<GameObject> cellList; //Список для хранения всех существующих клеток


    public void Awake()
    {
        S = this;
    }
    private void Start()
    {
        GenerateAquarium();

        CreateCell(0,0);

        InvokeRepeating("ShowStatus", 1, 1);

    }

    
    public void CreateCell(float x, float y)
    {
        GameObject newCell = Instantiate<GameObject>(cellPrefab);
        newCell.transform.position = new Vector3(x+0.5f * width, y+0.5f * height, 0f);
        newCell.GetComponent<CellMain>().cell_type = CellType.none;
        newCell.GetComponent<Renderer>().material.color = defaultColor;
        newCell.GetComponent<CellMain>().energyStoke = startEnergy;
        AddRigid(newCell);
        cellWasAtAll++;
        cellAlive++;
    }
    void ShowStatus()
    {
        print("Cells status: Was generated - " + cellWasAtAll + ", Alive - " + cellAlive + " , Died - " + cellDied);
    }


    public void GenerateAquarium()
    {
        GameObject leftWall = Instantiate<GameObject>(brickPrefab);
        leftWall.transform.position = new Vector3(-wallThickness*0.5f, 0.5f * height);
        leftWall.transform.localScale = new Vector3(wallThickness, height);
        //leftWall.GetComponent<BoxCollider>().size = leftWall.transform.localScale;

        GameObject rightWall = Instantiate<GameObject>(brickPrefab);
        rightWall.transform.position = new Vector3(width+wallThickness * 0.5f, 0.5f * height);
        rightWall.transform.localScale = new Vector3(wallThickness, height);
        //rightWall.GetComponent<BoxCollider>().size = rightWall.transform.localScale;

        GameObject bottomSlab = Instantiate<GameObject>(brickPrefab);
        bottomSlab.transform.position = new Vector3(0.5f*width, -wallThickness * 0.5f);
        bottomSlab.transform.localScale = new Vector3(width, wallThickness);
        //bottomSlab.GetComponent<BoxCollider>().size = bottomSlab.transform.localScale;

    }

    public void AddRigid(GameObject go)
    {
        go.AddComponent<Rigidbody>();
        //go.GetComponent<Rigidbody>().useGravity = false;
        go.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX;
        go.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationY;
        go.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ;
    }
}
