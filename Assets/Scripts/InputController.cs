using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    public enum KeyType { F,B,L,R }

    public GameObject agentCube;
    public GameObject agentSphere;
    public Button F_Btn;
    public Button B_Btn;
    public Button L_Btn;
    public Button R_Btn;
    public Button undoBtn;
    public Button redoBtn;
    public Button cubeBtn;
    public Button sphereBtn;
    public GameObject commandPrefab;
    public RectTransform commandListContent;

    private GameObject agent;

    private KeyCode ForwardKey = KeyCode.W;
    private KeyCode BackKey = KeyCode.S;
    private KeyCode LeftKey = KeyCode.A;
    private KeyCode RightKey = KeyCode.D;

    /// <summary>
    /// 用来标识目前修改的是哪个按键
    /// </summary>
    private KeyType keyType;

    /// <summary>
    /// 用来标识目前是否处于修改按键状态
    /// </summary>
    private bool isGettingKeyCode;

    /// <summary>
    /// 响应式队列 撤销栈
    /// </summary>
    private IReactiveCollection<ICommand> UndoStack = new ReactiveCollection<ICommand>();

    /// <summary>
    /// 响应式队列 还原栈
    /// </summary>
    private IReactiveCollection<ICommand> RedoStack = new ReactiveCollection<ICommand>();
    private IReactiveProperty<int> commandIndex = new ReactiveProperty<int>(-1);
    private Dictionary<int, Image> commandImagesDic = new Dictionary<int, Image>();

    public void Undo()
    {
        if (UndoStack.Count > 0)
        {
            ICommand command = UndoStack[UndoStack.Count - 1];
            command.undo();
            UndoStack.Remove(command);
            commandImagesDic[commandIndex.Value].color = Color.white;
            commandIndex.Value--;
        }
    }

    public void Redo()
    {
        if (RedoStack.Count > 0)
        {
            ICommand command = RedoStack[RedoStack.Count - 1];
            command.excute();
            RedoStack.Remove(command);
            if (commandIndex.Value > -1)
            {
                commandImagesDic[commandIndex.Value].color = Color.white;
            }
            commandIndex.Value++;
        }
    }

    public void ChangeKeyCode(KeyType type)
    {
        keyType = type;
        isGettingKeyCode = true;
    }

    private void Awake()
    {
        agent = agentCube;

        F_Btn.onClick.AddListener(() => ChangeKeyCode(KeyType.F));
        B_Btn.onClick.AddListener(() => ChangeKeyCode(KeyType.B));
        L_Btn.onClick.AddListener(() => ChangeKeyCode(KeyType.L));
        R_Btn.onClick.AddListener(() => ChangeKeyCode(KeyType.R));

        undoBtn.onClick.AddListener(Undo);
        redoBtn.onClick.AddListener(Redo);

        cubeBtn.onClick.AddListener(() => agent = agentCube);
        sphereBtn.onClick.AddListener(() => agent = agentSphere);

        UndoStack.ObserveCountChanged().Subscribe(x => undoBtn.interactable = x > 0 ? true : false);
        UndoStack.ObserveRemove().Subscribe(x => RedoStack.Add(x.Value));

        RedoStack.ObserveCountChanged().Subscribe(x => redoBtn.interactable = x > 0 ? true : false);
        RedoStack.ObserveRemove().Subscribe(x => UndoStack.Add(x.Value));

        commandIndex.TakeUntilDestroy(gameObject).Subscribe(x =>
        {
            if (x > -1)
            {
                commandImagesDic[x].color = Color.red;
            }
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(ForwardKey))
        {
            Move(new MoveForward(agent));
        }
        if (Input.GetKeyDown(BackKey))
        {
            Move(new MoveBack(agent));
        }
        if (Input.GetKeyDown(LeftKey))
        {
            Move(new MoveLeft(agent));
        }
        if (Input.GetKeyDown(RightKey))
        {
            Move(new MoveRight(agent));
        }
    }

    private void OnGUI()
    {
        //修改按键
        if (Input.anyKeyDown && isGettingKeyCode)
        {
            Event e = Event.current;
            if (e.isKey && e.keyCode != KeyCode.None)
            {
                switch (keyType)
                {
                    case KeyType.F:
                        ForwardKey = e.keyCode;
                        F_Btn.GetComponentInChildren<Text>().text = e.keyCode.ToString();
                        break;
                    case KeyType.B:
                        BackKey = e.keyCode;
                        B_Btn.GetComponentInChildren<Text>().text = e.keyCode.ToString();
                        break;
                    case KeyType.L:
                        LeftKey = e.keyCode;
                        L_Btn.GetComponentInChildren<Text>().text = e.keyCode.ToString();
                        break;
                    case KeyType.R:
                        RightKey = e.keyCode;
                        R_Btn.GetComponentInChildren<Text>().text = e.keyCode.ToString();
                        break;
                    default:
                        break;
                }
                isGettingKeyCode = false;
            }
        }
    }

    /// <summary>
    /// 执行移动操作
    /// </summary>
    /// <param name="move"></param>
    private void Move(ICommand move)
    {
        #region 将还原栈和相关UI清空

        var removeList = new List<int>();
        foreach (var item in commandImagesDic.Keys)
        {
            if (item > commandIndex.Value)
            {
                removeList.Add(item);
            }
        }
        foreach (var item in removeList)
        {
            Destroy(commandImagesDic[item].gameObject);
            commandImagesDic.Remove(item);
        }
        RedoStack.Clear();

        #endregion

        //将命令添加到撤销栈中
        UndoStack.Add(move);

        //执行命令
        move.excute();

        AddCommand();

        commandIndex.Value++;
    }

    /// <summary>
    /// 添加命令可视化UI
    /// </summary>
    private void AddCommand()
    {
        GameObject go = Instantiate<GameObject>(commandPrefab, commandListContent);
        Image image = go.GetComponent<Image>();
        if (commandIndex.Value > -1)
        {
            commandImagesDic[commandIndex.Value].color = Color.white;
        }
        commandImagesDic.Add(commandIndex.Value + 1, image);
    }
}
