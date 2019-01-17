using System.Collections;
using System.Collections.Generic;

namespace Evolution {
    
    public class EvalInfo
    {
        public float Height { get { return height; } }
        public float SpreadDegree { get { return spreadDegree; } }
        public int BranchNum { get { return branchNum; } }
        public float RadiusAttenuation { get { return radiusAttenuation; } }
        public float BranchDetail { get { return branchDetail; } }
        private float height; // 単純に高さ
        private float spreadDegree; // 広がり
        private int branchNum; // 枝の数
        private float radiusAttenuation; // どれぐらいの attenuation だったか
        private float branchDetail; // ブランチの細かさ. 小さい 枝が先端にある方が綺麗だと思ったので

        public EvalInfo(float h, float sg, int bn, float ra, float bd) {
            height = h;
            spreadDegree = sg;
            branchNum = bn;
            radiusAttenuation = ra;
            branchDetail = bd;
        }

    }
}
