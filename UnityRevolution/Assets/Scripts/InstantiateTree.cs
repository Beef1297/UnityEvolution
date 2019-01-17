using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Evolution;
using System.Threading;

public class InstantiateTree : MonoBehaviour
{
    [SerializeField] private GameObject[] trees;
    [SerializeField] private float SpawnRange = 40;
    private List<LSystemTree> lsystemTrees;
    private LSystem ls;
    private int N = 4;
    private EvalInfo evalInfo;
    private bool[] updateRules;

    private Thread evalThread;
    private bool isRunning = false;
    private bool resetSyntax = false;
    private readonly ManualResetEvent mre = new ManualResetEvent(false);
    // Start is called before the first frame update
    void Start()
    {
        updateRules = new bool[trees.Length];
        for(int i = 0; i < updateRules.Length; i++) {
            updateRules[i] = false;
        }
        StartCoroutine("SpawnTrees");
        evalThread = new Thread(EvalutionTree);
        evalThread.IsBackground = true;
        evalThread.Start();

    }

    IEnumerator SpawnTrees () {
        while(true) {
            try {
                ls = LSystem.Instance;
                
                if (ls.N < this.N) {
                    ls.UpdateRuleByNumber(this.N - ls.N);
                } else if (resetSyntax) {
                    ls.ResetRule("X");
                    resetSyntax = false;
                    continue;
                }
                for (int i = 0; i < trees.Length; i++) {
                    float x = Random.Range(-SpawnRange, SpawnRange);
                    float z = Random.Range(-SpawnRange, SpawnRange);
                    var to = Instantiate(trees[i], new Vector3(x, -20, z), Quaternion.identity, transform);
                    Destroy(to, 10f * Random.Range(0.5f, 2.0f));
                    evalInfo = ls.EvaluationInfo;
                    if (evalInfo != null) {
                        updateRules[i] = true;
                        EvalThreadRun(); // 一つでも true になったら start
                    }
                }

            } catch(System.NullReferenceException e) {
                Debug.LogWarning("Error occured: (spawn trees)" + e);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void EvalutionTree() {
        System.Random rnd = new System.Random();
        const int STANDARD_SIZE = 80;
        mre.WaitOne();

        try {
            var size = Mathf.Sqrt(evalInfo.SpreadDegree * evalInfo.Height);
            Debug.Log("size: " + size);
            int tableFCount = ls.Rule.TableF.Count;
            int elementsCount = ls.Rule.UsingRuleElements.Count;
            if (size <= STANDARD_SIZE) { // 観測結果より
                var randomIndex = rnd.Next(tableFCount - 1);
                string rule = "F";
                for (int i = 0; i < 4; i++) {
                    rule += ls.Rule.UsingRuleElements[rnd.Next(elementsCount - 1)];
                }
                if (evalInfo.Height <= 150) rule += "F-F";
                if (evalInfo.SpreadDegree <= 100) rule += "[-F[+F]]";
                ls.Rule.TableF[randomIndex] = rule;
            } else if (evalInfo.BranchNum <= 500) {
                var randomIndex = rnd.Next(tableFCount - 1);
                string rule = "F";
                for (int i = 0; i < 4; i++) {
                    rule += ls.Rule.UsingRuleElements[rnd.Next(elementsCount - 1)];
                }
                ls.Rule.TableF[randomIndex] = rule;
            }
            resetSyntax = true;
        } 
        finally {
            isRunning = false;

            mre.Reset();
            evalThread = new Thread(EvalutionTree);
            evalThread.IsBackground = true;
            evalThread.Start();
        }
    }

    // スレッド再開
    void EvalThreadRun() {
        isRunning = true;
        mre.Set();
    }
}
