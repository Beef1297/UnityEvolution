using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Evolution;
using System.Threading;

public class InstantiateTree : MonoBehaviour
{
    [SerializeField] private GameObject[] trees;
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
        float x = 0f;
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
                    var to = Instantiate(trees[i], new Vector3(x, 0, 0), Quaternion.identity, transform);
                    evalInfo = ls.EvaluationInfo;
                    if (evalInfo != null) {
                        updateRules[i] = true;
                        EvalThreadRun(); // 一つでも true になったら start
                    }
                }

                x += 10f;
            } catch(System.NullReferenceException e) {
                Debug.LogWarning("Error occured: (spawn trees)" + e);
            }
            yield return new WaitForSeconds(2f);
        }
    }

    void EvalutionTree() {
        mre.WaitOne();

        try {
            var size = Mathf.Sqrt(evalInfo.SpreadDegree * evalInfo.Height);
            Debug.Log("size: " + size);
            if (size <= 80) {
                var randomIndex = Mathf.RoundToInt(size) % ls.Rule.TableF.Count;
                string rule = ls.Rule.TableF[randomIndex];
                rule += "+F";
                ls.Rule.TableF[0] = rule;
            }
            if (evalInfo.BranchNum <= 500) {
                var randomIndex = Mathf.RoundToInt(evalInfo.BranchNum) % ls.Rule.TableF.Count;
                string rule = ls.Rule.TableF[randomIndex];
                rule += "[-F]";
                ls.Rule.TableF[0] = rule;
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
