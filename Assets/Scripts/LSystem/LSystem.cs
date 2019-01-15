using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Evolution {

    [System.Serializable]
    public class LSystem
    {
        private int generations;
        public int Generations { get { return generations; } }
        public int Height { get { return height; } }
        private int height;
        private float angle;
        public float Angle { get {return angle; } }
        private RepresentRule rule;
        public RepresentRule Rule { get { return rule; } } 
        private float branchLength; // TODO: 実装

        private string s_brackets;
        public string S_Brackets { get { return s_brackets; } }
        
        public char[] BracketsCharList { get { return bracketsCharList; } }
        private char[] bracketsCharList;
        private int index = 0;
        
        public LSystem (int n, string init = "F") {
            s_brackets = init;
            angle = 25.7f;
            height = 1;
            rule = new RepresentRule();
            for (int i = 0; i < n; i++) {
                Update();
            }
            CalcHeight(true);
            bracketsCharList = s_brackets.ToCharArray();
        }

        public char GetCharByIndex() {
            return s_brackets[index++];
        }

        public void Update () {
            string next = ""; // 新しい文字列を作っていってしまう．
            var str_c = s_brackets.ToCharArray();
            for (int i = 0; i < str_c.Length; i++) {
                if (rule.Table.ContainsKey(str_c[i])) {
                    next += rule.Table[str_c[i]];
                    continue;
                }
                next += str_c[i];
            }
            s_brackets = next;
        }

        public int CalcHeight(bool needsUpdate) {
            if (needsUpdate) {
                var heights = new List<int>();
                _calcHeight(s_brackets, heights, 0);

                for (int i = 0; i < heights.Count;i ++) {
                    this.height = Mathf.Max(this.height, heights[i]);
                }
                this.height++;
            }

            return this.height;

        }

        private int _calcHeight(string str, List<int> heights, int i) {
            if (i >= str.Length - 1) {
                return i;
            }
            int h = 0;
            while(i < str.Length && str[i] != ']') {
                if (str[i] == 'F') {
                    h++;
                }
                if (str[i] == '[') {
                    i = _calcHeight(str, heights, i+1);
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

        public int GetLength() {
            return bracketsCharList.Length;
        }

        public void PrintString() {
            Debug.Log("Height is : " + height + ", sysntax is : " + s_brackets);
        }

    }


    public class RepresentRule {
        public Dictionary<char, string> Table { get { return table; } }
        public char[] UsingChars { get { return usingChars; } }
        Dictionary<char, string> table = new Dictionary<char, string>() {
            {'F', "F[+F]F[-F]F"},
        };

        char[] usingChars = new char[] {
            'F',
            '[',
            ']',
            '+',
            '-'
        };
        public RepresentRule() {}
        

    }

}
