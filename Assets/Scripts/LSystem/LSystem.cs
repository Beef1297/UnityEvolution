using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Evolution {

    public class LSystem : SingletonMonoBehaviourFast<LSystem>
    {
        private int generations = 1;
        public int Generations { get { return generations; } }
        public int MaxLength { get { return maxLength; } }
        private int maxLength = 1;
        private float angle;
        public float Angle { get {return angle; } }
        private RepresentRule rule;
        public RepresentRule Rule { get { return rule; } } 

        public EvalInfo EvaluationInfo { get { return evalInfo; } }
        private EvalInfo evalInfo;

        public int N { 
            get { return n; }
        }
        private float branchLength; // TODO: 実装

        private string s_brackets;
        public string S_Brackets { get { return s_brackets; } }
        private int n = 0;
        
        protected override void Start () {
            s_brackets = "F";
            maxLength = 1;
            rule = new RepresentRule();
            angle = rule.angle;
            //
            UpdateRuleByNumber(n);
            Debug.Log("This is the start of LSystem");
        }

        public void UpdateRuleByNumber(int number) {
            for (int i = 0; i < number; i++) {
                UpdateRule();
            }
        }

        public void UpdateRule () {
            string next = ""; // 新しい文字列を作っていってしまう．
            if (s_brackets == null) return;
            for (int i = 0; i < s_brackets.Length; i++) {
                if (s_brackets[i] == 'F') {
                    int randomIndex = Mathf.RoundToInt(Random.Range(0, 5 * (rule.TableF.Count - 1))) % rule.TableF.Count;
                    next += rule.TableF[randomIndex];
                    continue;
                }
                if (s_brackets[i] == 'X') {
                    next += rule.TableX[Mathf.RoundToInt(Random.Range(0, (rule.TableX.Count - 1)))];
                    continue;
                }
                next += s_brackets[i];
            }
            s_brackets = next;
            this.n++; // n を update するごとに更新 他でも今どういう状態か確認できるようにするため
            CalcMaxLength(true);
        }

        // F の長さを測定
        public int CalcMaxLength(bool needsUpdate) {
            if (needsUpdate) {
                var heights = new List<int>();
                _calcMaxLength(s_brackets, heights, 0);

                for (int i = 0; i < heights.Count;i ++) {
                    this.maxLength = Mathf.Max(this.maxLength, heights[i]);
                }
                this.maxLength++;
            }

            return this.maxLength;

        }

        private int _calcMaxLength(string str, List<int> heights, int i) {
            if (i >= str.Length - 1) {
                return i;
            }
            int h = 0;
            while(i < str.Length && str[i] != ']') {
                if (str[i] == 'F') {
                    h++;
                }
                if (str[i] == '[') {
                    i = _calcMaxLength(str, heights, i+1);
                }
                i++;
            }
            heights.Add(h);
            // string s = "";
            // for (int j = 0;j < i; j++) {
            //     s += str_c[j];
            // }
            // Debug.Log("Process: " + s);
            return i;
        }

        public void ChangeRule() {

        }

        public void RecreateRule(int k) {
            ResetRule();
            for (int i = 0; i < k; i++) {
                UpdateRule();
            }
        }

        public void ResetRule(string init="F") {
            this.n = 0;
            this.s_brackets = init;
            this.angle = rule.angle;
        }

        public void SetEvalInfo(float height, float spread, int branchNum, float rAttenuation, float branchDetail) {
            evalInfo = new EvalInfo(height, spread, branchNum, rAttenuation, branchDetail);
        }

        public void PrintRule() {
            Debug.Log("maxLength is : " + maxLength + ", sysntax is : " + s_brackets);
        }

    }


    public class RepresentRule {
        public List<string> TableF { 
            get { return tableF; } 
        }
        public List<string> TableX { get { return tableX; } }
        public char[] UsingChars { get { return usingChars; } }
        public float angle = 25.7f;
        // 生成規則は基本的に 1文字 (X or F) -> 文字列にする．面倒なので
        // だから，List で一括管理
        List<string> tableF = new List<string>() {
            "F[+F]F[-F]F",
            "F[+F]F",
            "F[−F]F",
        };
        List<string> tableX = new List<string>(){
            "F[+X]F[-X]+X"
        };

        char[] usingChars = new char[] {
            'F',
            '[',
            ']',
            '+',
            '-'
        };
        public RepresentRule() {}
        
        public void ChangeFRuleByIndex(int index, string rule) {
            tableF[index] = rule;
        }
    }

}
