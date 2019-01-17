using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Evolution {

	public class LSystemTree : ProceduralModelingBase {

		public TreeData Data { 
            get { return data; } 
        } 

		[SerializeField] TreeData data;

		// 木の枝が分岐する深さ（木の枝が根元から分岐する世代数）
		protected int generations = 1;

		// 木の枝の基本となる長さ（根元の枝の長さ）
		[SerializeField, Range(0.5f, 5f)] protected float length = 1f;

		// 木の枝の基本となる太さ（根元の枝の太さ）
		[SerializeField, Range(0.1f, 2f)] protected float radius = 0.15f;

        int branchNum = 0;
        float treeHeight = 0f;
        float spreadDeg = 0f;
        float branchDetail = 0f;
		const float PI2 = Mathf.PI * 2f;


		public Mesh Build(TreeData data, int generations, float length, float radius) {
			data.Setup();
            generations = data.lsystem.MaxLength;
			var root = new TreeBranch(
				generations, 
				length, 
				radius, 
				data
			);
            /*
             TreeBranch 再帰させないで，枝を一本生成して返すっていう風にする．対象となってる treebranch を持つようにして
             それの子に枝を追加するかどうかみたいなのを作って行けばいい．
             子に追加するためには，親の情報を渡すようにすればいい．root だけは作った方がいいかな．結局ランダム性があるんだkら
             生成してみないとワカらないよね．
             */

            string generator = data.lsystem.S_Brackets; //
            Debug.Log("Length is : " + generator.Length + " generator is: " + generator);
            var parent = root;
            List<TreeBranch> parents = new List<TreeBranch>(); // 親をスタックしていく
            List<float> rotations = new List<float>();
            float rotation_b = 0f;
            branchNum = 0;
            for (int i = 1; i < generator.Length; i++) { // root は除く

                if (generator[i] == 'F') {

                    TreeBranch newBranch = new TreeBranch(
                        parent.Generation - 1, 
                        generations,
                        parent.To,
                        parent.SegmentForChild.Frame.Tangent,
                        parent.SegmentForChild.Frame.Normal,
                        parent.SegmentForChild.Frame.Binormal,
                        parent.Length * data.lengthAttenuation,
                        parent.ToRadius,
                        parent.Offset + parent.Length,
                        rotation_b,
                        data
                    );
                    if (newBranch.FromRadius <= 0.01f) {
                        continue;
                    }
                    if (newBranch.FromRadius <= 0.5f) {
                        branchDetail++; // TODO: もっと条件を追加する
                    }
                    if (Vector3.Distance(root.SegmentForChild.Position, parent.SegmentForChild.Position) > treeHeight) {
                        // 見たいのは相対的な評価ならこれでも大丈夫
                        treeHeight = parent.SegmentForChild.Position.y; // (単純な高さじゃなくて，どれぐらい伸びたのかというのが正しい)
                    }
                    parent.Children.Add(newBranch);
                    branchNum++;
                    parent = newBranch;

                } else if (generator[i] == '+') {
                    rotation_b = data.lsystem.Angle; // TODO: method 使う
                    spreadDeg++; // FIXME: 今は回転したら広がっているとしている
                } else if (generator[i] == '-') {
                    rotation_b = -data.lsystem.Angle;
                    spreadDeg++;
                } else if (generator[i] == '[') {
                    parents.Add(parent);
                    rotations.Add(rotation_b);
                } else if(generator[i] == ']') {
                    // ']' の時
                    // parent を pop する
                    if (parents.Count <= 0) {
                        Debug.LogError("At Close Bracket, somethin is wrong, maybe it's rule");
                    }
                    parent = parents[parents.Count - 1];
                    parents.RemoveAt(parents.Count - 1);
                    // 親の回転より回転を元に戻す
                    // -> 親がもともと曲がっててて，それに対して F はその曲がったまま伸びるため
                    rotation_b = rotations[rotations.Count - 1];
                    rotations.RemoveAt(rotations.Count - 1);
                    
                }
            }
            Debug.Log(branchNum);

            data.lsystem.SetEvalInfo(treeHeight, spreadDeg, branchNum, data.radiusAttenuation, branchDetail);

			var vertices = new List<Vector3>();
			var normals = new List<Vector3>();
			var tangents = new List<Vector4>();
			var uvs = new List<Vector2>();
			var triangles = new List<int>();

			// 木の全長を取得
			// 枝の長さを全長で割ることで、uv座標の高さ(uv.y)が
			// 根元から枝先に至るまで[0.0 ~ 1.0]で変化するように設定する
			float maxLength = TraverseMaxLength(root);

			// 再帰的に全ての枝を辿り、一つ一つの枝に対応するMeshを生成する
			Traverse(root, (branch) => {
				var offset = vertices.Count;

				var vOffset = branch.Offset / maxLength;
				var vLength = branch.Length / maxLength;

				// 一本の枝から頂点データを生成する
				for(int i = 0, n = branch.Segments.Count; i < n; i++) {
					var t = 1f * i / (n - 1);
					var v = vOffset + vLength * t;

					var segment = branch.Segments[i];
					var N = segment.Frame.Normal;
					var B = segment.Frame.Binormal;
					for(int j = 0; j <= data.radialSegments; j++) {
						// 0.0 ~ 2π
						var u = 1f * j / data.radialSegments;
						float rad = u * PI2;

						float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
						var normal = (cos * N + sin * B).normalized;
						vertices.Add(segment.Position + segment.Radius * normal);
						normals.Add(normal);

						var tangent = segment.Frame.Tangent;
						tangents.Add(new Vector4(tangent.x, tangent.y, tangent.z, 0f));

						uvs.Add(new Vector2(u, v));
					}
				}

				// 一本の枝の三角形を構築する
				for (int j = 1; j <= data.heightSegments; j++) {
					for (int i = 1; i <= data.radialSegments; i++) {
						int a = (data.radialSegments + 1) * (j - 1) + (i - 1);
						int b = (data.radialSegments + 1) * j + (i - 1);
						int c = (data.radialSegments + 1) * j + i;
						int d = (data.radialSegments + 1) * (j - 1) + i;

						a += offset;
						b += offset;
						c += offset;
						d += offset;

						triangles.Add(a); triangles.Add(d); triangles.Add(b);
						triangles.Add(b); triangles.Add(d); triangles.Add(c);
					}
				}
			});

			var mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
            Debug.Log("vertices's count: " + vertices.Count);
			mesh.normals = normals.ToArray();
			mesh.tangents = tangents.ToArray();
			mesh.uv = uvs.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.RecalculateBounds();


			return mesh;
		}

		protected override Mesh Build ()
		{
            var mesh = Build(data, generations, length, radius);
            return mesh;
		}

		// 木の枝を再帰的に辿り、全長の長さ（根元から枝先に至るまでの長さ）を返す
		static float TraverseMaxLength(TreeBranch branch) {
			float max = 0f;
			branch.Children.ForEach(c => {
				max = Mathf.Max(max, TraverseMaxLength(c));
			});
			return branch.Length + max;
		}

		// 木の枝を再帰的に辿る
		// 到達した枝に対してAction<TreeBranch> actionコールバックを実行する
		static void Traverse(TreeBranch from, Action<TreeBranch> action) {
			if(from.Children.Count > 0) {
				from.Children.ForEach(child => {
					Traverse(child, action);
				});
			}
			action(from);
		}


	}

	[System.Serializable]
	public class TreeData {
		public int randomSeed = 0;
		[Range(0.5f, 0.99f)] public float lengthAttenuation = 0.9f, radiusAttenuation = 0.6f;
		[Range(1, 3)] public int branchesMin = 1, branchesMax = 3;
        [Range(-45f, 0f)] public float growthAngleMin = -15f;
        [Range(0f, 45f)] public float growthAngleMax = 15f;
        [Range(1f, 10f)] public float growthAngleScale = 4f;
		[Range(3, 20)] public int radialSegments = 8;
        [Range(2, 5)] public int heightSegments = 2;
		[Range(0.0f, 0.35f)] public float bendDegree = 0.1f;
        [HeaderAttribute("LSystem Parameter")]
        [Range(1, 6)] public int N = 1;
        [SerializeField] protected bool updateByN = false; // Editor 用 bool 値

        public LSystem lsystem { 
            get { return ls; }
            set { ls = value; }
        }
        LSystem ls = null;

		// UnityEngine.Randomではなく、このRandクラスから生成された乱数を用いることで、
		// 同じTreeDataのパラメータであれば同じ形の木が生成できるようにしており、
		// 欲しい分岐パターンの木を生成しやすくしている。
		Rand rnd;

		public void Setup() {
            ls = LSystem.Instance;
            if (ls == null) {
                Debug.LogWarning("ls is null in data tree setup method");
            }
            if (updateByN) {
                int diff = N - ls.N;
                if (diff > 0) {
                    ls.UpdateRuleByNumber(diff);
                }
            }
			rnd = new Rand(randomSeed);
		}

		public int Range(int a, int b) {
			return rnd.Range(a, b);
		}

		public float Range(float a, float b) {
			return rnd.Range(a, b);
		}

		public int GetRandomBranches() {
			return rnd.Range(branchesMin, branchesMax + 1);
		}

		public float GetRandomGrowthAngle() {
			return rnd.Range(growthAngleMin, growthAngleMax);
		}

		public float GetRandomBendDegree() {
			return rnd.Range(-bendDegree, bendDegree);
		}
	}

	// 一つの枝を表現するクラス
	public class TreeBranch {
		public int Generation { get { return generation; } }
		public List<TreeSegment> Segments { get { return segments; } }
		public List<TreeBranch> Children { get { return children; } }

		public Vector3 From { get { return from; } }
		public Vector3 To { get { return to; } }
		public float Length { get { return length; } } 
		public float Offset { get { return offset; } }

        public float ToRadius { get { return toRadius; } }
        public float FromRadius { get { return fromRadius; } }
        public float BinormalRoation { get { return binormalRotaion; } }

        public TreeSegment SegmentForChild { get { return segmentForChild; } }

		// 自身の世代（0が枝先）
		int generation;

		// モデルを生成する際に必要な、枝を分割する節
		List<TreeSegment> segments;

		// 自身から分岐したTreeBranch
		List<TreeBranch> children;

		// 一本の枝の根元と先端の位置
		Vector3 from, to;

		// 根元の太さと先端の太さ
		float fromRadius, toRadius;

		// 枝の長さ
		float length;

		// 根元から自身の根元に至るまでの長さ
		float offset;
        float binormalRotaion;

        TreeSegment segmentForChild;

		// 根元のコンストラクタ
		public TreeBranch(
            int generations, 
            float length, 
            float radius, 
            TreeData data) : this(generations, generations, Vector3.zero, Vector3.up, Vector3.right, Vector3.back, length, radius, 0f, 0f, data) {
		}

		public TreeBranch(
            int generation, 
            int generations, 
            Vector3 from, 
            Vector3 tangent, 
            Vector3 normal, 
            Vector3 binormal, 
            float length, 
            float radius, 
            float offset, 
            float rotation_b, // binormal の回転量 ::TEST
            TreeData data
            ) {
			this.generation = generation;

			this.fromRadius = radius;

			// 枝先である場合は先端の太さが0になる
			this.toRadius = (generation == 0) ? 0f : radius * data.radiusAttenuation;

			this.from = from;

			// 枝先ほど分岐する角度が大きくなる
            var scale = Mathf.Lerp(1f, data.growthAngleScale, 1f - 1f * generation / generations);

			// normal方向の回転
			var qn = Quaternion.AngleAxis(scale * data.GetRandomGrowthAngle(), normal);

            this.binormalRotaion = rotation_b;
			// binormal方向の回転
			var qb = Quaternion.AngleAxis(scale * binormalRotaion, binormal);

			// 枝先が向いているtangent方向にqn * qbの回転をかけつつ、枝先の位置を決める
			this.to = from + (qn * qb) * tangent * length;

			this.length = length;
			this.offset = offset;

			// モデル生成に必要な節を構築
			segments = BuildSegments(data, fromRadius, toRadius, normal, binormal);
            
            segmentForChild = segments[segments.Count - 1];
            // 子を生成するときはこの フレネフレームを使う 先端から伸ばす

			children = new List<TreeBranch>();

		}

        /// フレネフレームを生成する．枝一つは 4つに分割されている

		List<TreeSegment> BuildSegments (TreeData data, float fromRadius, float toRadius, Vector3 normal, Vector3 binormal) {
			var segments = new List<TreeSegment>();

			var curve = new CatmullRomCurve();
			curve.Points.Clear();

			var length = (to - from).magnitude;
			var bend = length * (normal * data.GetRandomBendDegree() + binormal * data.GetRandomBendDegree());
			curve.Points.Add(from);
			curve.Points.Add(Vector3.Lerp(from, to, 0.25f) + bend); // ここで + bend をすると 枝が曲がる
			curve.Points.Add(Vector3.Lerp(from, to, 0.75f) + bend);
			curve.Points.Add(to);

			var frames = curve.ComputeFrenetFrames(data.heightSegments, normal, binormal, false);
			for(int i = 0, n = frames.Count; i < n; i++) {
				var u = 1f * i / (n - 1);
                var radius = Mathf.Lerp(fromRadius, toRadius, u);

				var position = curve.GetPointAt(u);
                //Debug.Log(position);
				var segment = new TreeSegment(frames[i], position, radius);
				segments.Add(segment);
			}
			return segments;
		}

	}

	public class TreeSegment {
		public FrenetFrame Frame { get { return frame; } }
		public Vector3 Position { get { return position; } }
        public float Radius { get { return radius; } }

		// TreeSegmentが向いている方向ベクトルtangent、
		// それと直交するベクトルnormal、binormalを持つFrenetFrame
		FrenetFrame frame;

		// TreeSegmentの位置
		Vector3 position;

		// TreeSegmentの幅(半径)
        float radius;

		public TreeSegment(FrenetFrame frame, Vector3 position, float radius) {
			this.frame = frame;
			this.position = position;
            this.radius = radius;
		}
	}

	public class Rand {
		System.Random rnd;

		public float value {
			get {
				return (float)rnd.NextDouble();
			}
		}

		public Rand(int seed) {
			rnd = new System.Random(seed);
		}

		public int Range(int a, int b) {
			var v = value;
			return Mathf.FloorToInt(Mathf.Lerp(a, b, v));
		}

		public float Range(float a, float b) {
			var v = value;
			return Mathf.Lerp(a, b, v);
		}
	}

}

