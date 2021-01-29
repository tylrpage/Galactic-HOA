using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    public GameObject TutorialPlayer;
    public GameObject Leaf;
    public GameObject Greg;
    public AnimationCurve GregMovementCurve;
    public Transform GregEndPosition;
    public float TimeItTakesForGregToReachDestination;
    public ScoreController ScoreController;
    public GameObject[] TutorialCards;
    public float CircleRadius = 4.75f;

    private Camera _camera;
    private Movement _movement;
    private LeafBlower _leafBlower;
    private AnimationController _animationController;
    private LeafController _leafController;
    private Animator _gregAnimator;
    private Vector3 _originalManagerPos;
    private Vector3 _originalLeafPos;

    private bool _newPhase = true;
    private bool _canBlow = false;

    private enum TutorialPhase
    {
        Move,
        MoveToCircle,
        LeafFall,
        Blow,
        GregIntro,
        Fined,
        Finish
    }

    private TutorialPhase _tutorialPhase;
    
    // Start is called before the first frame update
    void Start()
    {
        _tutorialPhase = TutorialPhase.Move;
        
        _camera = Camera.main;
        _movement = TutorialPlayer.GetComponent<Movement>();
        _leafBlower = TutorialPlayer.GetComponent<LeafBlower>();
        _animationController = TutorialPlayer.GetComponent<AnimationController>();
        _leafController = Leaf.GetComponent<LeafController>();
        _gregAnimator = Greg.GetComponent<Animator>();
        _originalManagerPos = Greg.transform.position;
        ScoreController.enabled = false;
        _originalLeafPos = Leaf.transform.position;

        _animationController.SetSpriteDirection(true);
    }

    void SetTutorialCardActive(int index)
    {
        foreach (var tutorialCard in TutorialCards)
        {
            tutorialCard.SetActive(false);
        }
        TutorialCards[index].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        Inputs inputs = new Inputs();

        if (_newPhase)
        {
            _newPhase = false;
            switch (_tutorialPhase)
            {
                case TutorialPhase.Move:
                    SetTutorialCardActive(0);
                    break;
                case TutorialPhase.LeafFall:
                    SetTutorialCardActive(2);
                    StartCoroutine(LeafFallRoutine());
                    DropLeaf();
                    break;
                case TutorialPhase.Blow:
                    SetTutorialCardActive(4);
                    _canBlow = true;
                    StartCoroutine(BlowRoutine());
                    break;
                case TutorialPhase.GregIntro:
                    _canBlow = false;
                    SetTutorialCardActive(6);
                    StartCoroutine(BringInGregRoutine());
                    break;
                case TutorialPhase.Fined:
                    ScoreController.enabled = true;
                    break;
                case TutorialPhase.Finish:
                    SetTutorialCardActive(11);
                    break;
            }
        }
        
        PollInputs(ref inputs);
        
        if (_tutorialPhase == TutorialPhase.Move)
        {
            if (inputs.W || inputs.A || inputs.S || inputs.D)
            {
                StartCoroutine(MovingRoutine());
                _tutorialPhase = TutorialPhase.MoveToCircle;
                _newPhase = true;
            }
        }
        else if (_tutorialPhase == TutorialPhase.MoveToCircle)
        {
            if (InCircle())
            {
                _tutorialPhase = TutorialPhase.LeafFall;
                _newPhase = true;
            }
        }
        else if (_tutorialPhase == TutorialPhase.LeafFall)
        {
            inputs = Inputs.EmptyInputs();
        }
        else if (_tutorialPhase == TutorialPhase.Blow)
        {
            if (_originalLeafPos != Leaf.transform.position)
            {
                StartCoroutine(BlowRoutinePostBlow());
            }
        }
        else if (_tutorialPhase == TutorialPhase.GregIntro)
        {
            inputs = Inputs.EmptyInputs();
        }
        else if (_tutorialPhase == TutorialPhase.Fined)
        {
            inputs = Inputs.EmptyInputs();
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                StartCoroutine(FinedRoutine());
            }
        }
        else if (_tutorialPhase == TutorialPhase.Finish)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PlayerPrefs.SetInt("playedBefore", 1);
                SceneManager.LoadScene("SampleScene");
            }
        }

        _movement.SetInputs(inputs);
        _leafBlower.SetInputs(inputs.Space, inputs.MouseDir);
        _animationController.SetFace(inputs.Space);
    }

    private bool InCircle()
    {
        float distance = (TutorialPlayer.transform.position - Vector3.zero).magnitude;
        return distance <= CircleRadius;
    }

    private IEnumerator MovingRoutine()
    {
        yield return new WaitForSeconds(2f);
        SetTutorialCardActive(1);
    }

    private IEnumerator LeafFallRoutine()
    {
        yield return new WaitForSeconds(4f);
        SetTutorialCardActive(3);
        yield return new WaitForSeconds(4f);
        _tutorialPhase = TutorialPhase.Blow;
        _newPhase = true;
    }

    private IEnumerator BlowRoutine()
    {
        yield return new WaitForSeconds(4f);
        SetTutorialCardActive(5);
    }

    private IEnumerator BlowRoutinePostBlow()
    {
        yield return new WaitForSeconds(2f);
        _tutorialPhase = TutorialPhase.GregIntro;
        _newPhase = true;
    }

    private IEnumerator FinedRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        SetTutorialCardActive(9);
        yield return new WaitForSeconds(7f);
        SetTutorialCardActive(10);
        yield return new WaitForSeconds(7f);
        _tutorialPhase = TutorialPhase.Finish;
        _newPhase = true;
    }
    
    private void PollInputs(ref Inputs polledInputs)
    {
        if (Input.GetKey(KeyCode.W))
            polledInputs.W = true;
        if (Input.GetKey(KeyCode.A))
            polledInputs.A = true;
        if (Input.GetKey(KeyCode.S))
            polledInputs.S = true;
        if (Input.GetKey(KeyCode.D))
            polledInputs.D = true;
        if (Input.GetKey(KeyCode.Space))
            polledInputs.Space = true;
        if (!_canBlow)
            polledInputs.Space = false;

        polledInputs.MouseDir = ScreenToPlane() - (Vector2)TutorialPlayer.transform.position;
    }
    
    private Vector2 ScreenToPlane()
    {
        Vector2 viewport = _camera.ScreenToViewportPoint(Input.mousePosition);
        return new Vector2((viewport.x - 0.5f) * _camera.orthographicSize * _camera.aspect * 2, (viewport.y - 0.5f) * _camera.orthographicSize * 2);
    }
    
    private void DropLeaf()
    {
        Leaf.SetActive(true);
        _leafController.Simulate = true;
    }

    private IEnumerator BringInGregRoutine()
    {
        float t = 0;
        _gregAnimator.SetTrigger("run");
        while (t <= 1)
        {
            Greg.transform.position = Vector3.Lerp(_originalManagerPos, GregEndPosition.position, GregMovementCurve.Evaluate(t));
            if (t >= 0.8f)
            {
                _gregAnimator.SetTrigger("idle");
            }
            
            t += Time.deltaTime / TimeItTakesForGregToReachDestination;
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);
        SetTutorialCardActive(7);
        yield return new WaitForSeconds(5f);
        SetTutorialCardActive(8);
        _tutorialPhase = TutorialPhase.Fined;
        _newPhase = true;
    }
}
