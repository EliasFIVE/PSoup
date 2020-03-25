using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellType
{
    none,
    dead,
    plant,
    omni,
    vegan,
    carnivor,
    miner
}

public enum CellStatus
{
    free,
    birth,
    giveBirth,
    dying,
    none

}
public class CellMain : MonoBehaviour
{
    [SerializeField]
    private CellType type;                  //Тип этой клетки из списка    
    public Color color;                     //  Основной цвет клетки
    public GameObject parentCell = null;           // ссылка на родительскую клетку
    public float birthTime;                 // Время рождения, установлется во время Awake
    private Renderer rend;

    [Header("Color Settings")] //!!!! ПЕРЕНЕСТИ В РАЗДЕЛ УПРАВЛЕНИЯ ЦВЕТАМИ
    public Color deadColor;

    [Header("Constant parameters of the instance")]
    public float energyGeneration = 1f;      //Генерация энергии в секунду безусловная
    public float massToSize = 1f;  //Обратная величина от плотности, отношение массы к размеру: size = mass*massToSize
    public float fertileTreshold = 6f;       //Минимальная энергия при которой возможно деление
    public float fertileCooldownTime = 4f;   //Время между размножениями (косвенно определяет сколько будет потомков в течении жизни)
    public float maxLiveTime = 20f;          // Максимальный возраст жизни клетки
    
    [Header("Variable parameters of the instance")]
    public float energyStoke;               //Текущий запас энергии в клетке
    public float size;                      // Характерный размер клетки (диаметр)
    public float mass;                      // Масса, используется для задания массы в RigidBody
    public float lastReproductionTime;      //Время последнего размножения
    public float deathTime;                 // Время смерти
    [SerializeField]
    private CellStatus status;

    [Header("Temp parameters")]
    public float s1;
    public float s2;

    public CellType cell_type
    {
        get { return type; }
        set { type = value; }
    }
    public CellStatus cell_satus
    {
        get { return status; }
        set { status = value; }
    }

    private void Awake()
    {
        Main.S.cellList.Add(this.gameObject);

        rend = this.GetComponent<Renderer>();
        birthTime = Time.time;        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        ReSize();

        if (type == CellType.dead)
        {
            rend.material.color = Color.white;
        }
        //energyStoke = energyInitial;

        //  !!!! заменить на функцию роста GrowUp(), которая включает рост объема в зависимости от energyStoke  и масси твердого тела (с учетом плотности);
        //size = 1;                                    
    }

    // Update is called once per frame
    void Update()
    {

        //Проверить, жива ли клетка
        if (type != CellType.dead)
        {
            switch (status)
            {
                case CellStatus.free:
                    //Проверка возраста смерти
                    if (Time.time > (birthTime + maxLiveTime))
                    {
                        Death();
                        break;
                    }
                    //Обновить состояние клетки, а именно создать энергию и !!!доделать - вырасти
                    GenerateEnergy();
                    ReSize();
                    //Проверка условий для размножения
                    if ((Time.time - lastReproductionTime) > fertileCooldownTime)
                    {
                        if (energyStoke >= fertileTreshold)
                        {
                            InitiateBirth();
                        }
                    }
                    break;

                case CellStatus.birth:
                    BirthProcessChild();
                    break;

                case CellStatus.giveBirth:
                    BirthProcessParent();
                    break;

                case CellStatus.dying:
                    Dying(rend.material.color,s1,s2);
                    break;

                default:
                    print("ERROR! There is no case fo this CellStatus");
                    break;
            }
        }
    }

    // Получение энергии со временем !!! Позже добавить фотосинтез
    void GenerateEnergy()
    {
        if (type == CellType.none)
        {
            energyStoke += energyGeneration * Time.deltaTime;
        }
    }

    //Обновление размера и массы в соответствии со значением энергии и плотности
    void ReSize()
    {
        mass = energyStoke * Main.S.energyToMass;
        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            gameObject.GetComponent<Rigidbody>().mass = mass;
        }
        else { print("Resise Cell with no Rigid body"); }
        size = mass * massToSize;
        transform.localScale = Vector3.one * size;
    }

    //Родить новую клетку !!! позже реализовать наследование свойств родителя со случайными отклонениями
    void InitiateBirth()
    {
        //Зафиксировать время рождения и глобальный статус
        status = CellStatus.giveBirth;
        lastReproductionTime = Time.time;
        Main.S.cellAlive++;
        Main.S.cellWasAtAll++;
        
        //Создать новую клетку и настроить ее параметры        
        GameObject newCell = Instantiate<GameObject>(Main.S.cellPrefab);
        newCell.GetComponent<CellMain>().birthTime = Time.time;
        newCell.GetComponent<CellMain>().status = CellStatus.birth;
        newCell.GetComponent<CellMain>().energyStoke = energyStoke * Main.S.energyInitial;
        newCell.GetComponent<CellMain>().parentCell = this.gameObject;
        newCell.transform.SetParent(this.gameObject.transform);
        //Разместить новую клетку внутри родителя
        Vector3 birthDirection = Random.insideUnitSphere;
        birthDirection.z = 0;
        Vector3 birthDistance = birthDirection.normalized * Main.S.birthDistAddition*1.5f;
        newCell.transform.localPosition = birthDistance;
        //Отмасштабировать клетку внутри родителя
        // newCell.transform.localScale = Vector3.one * Main.S.energyInitial;

        //Заплатить за клетку
        //energyStoke -= newCell.GetComponent<CellMain>().energyStoke * Main.S.fertileEnergyCost;

        //newCell.gameObject.GetComponent<CellMain>().cell_type = CellType.dead; //Родить мертвую клетку
    }
    
    void BirthProcessChild()
    {
        float u = (Time.time - birthTime) / Main.S.borningTime;
 
        if (u <= 1)
        {
            float dist = Main.S.birthDistAddition * 1.5f + u * (Main.S.birthDistIndependence - Main.S.birthDistAddition * 1.5f);
            Vector3 pos = transform.localPosition.normalized * dist;
            transform.localPosition = pos;
            transform.localScale = Vector3.one * (size / parentCell.GetComponent<CellMain>().size);
        } else
        {
            birthTime = Time.time;            
            parentCell.GetComponent<CellMain>().cell_satus = CellStatus.free;
            float payment = energyStoke * Main.S.fertileEnergyCost;
            parentCell.GetComponent<CellMain>().energyStoke -= payment;
            this.gameObject.transform.SetParent(null);
            Main.S.AddRigid(this.gameObject);
            status = CellStatus.free;
        }        
    }

    void BirthProcessParent()
    {
        return;
    }
    // Умереть
    void Death()
    {
        //Destroy(this.gameObject);
        deathTime = Time.time;
        status = CellStatus.dying;
        s1 = massToSize;
        s2 = Main.S.deadSizeRatio * s1;
        Main.S.cellAlive--;
        Main.S.cellDied++;
    }
    void Dying(Color c0, float s0, float s1)
    {
        float u = (Time.time - deathTime) / Main.S.dyingTime;
        if (u <= 1)
        {
            color = Color.Lerp(c0, deadColor, u);
            rend.material.color = color;
            massToSize = s0 + u * (s1 - s0);
            ReSize();
        } else
        {
            type = CellType.dead;
            status = CellStatus.free;
        }
    }
}
