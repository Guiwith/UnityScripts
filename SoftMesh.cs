using UnityEngine;

public class SoftMesh : MonoBehaviour
{
    // 描述我们的软体对象将弹跳的速度
    public float bounceSpeed;
    // 物体下落的力量
    public float fallForce;

    // 我们需要这个值来最终停止来回弹跳。
    public float stiffness;

    // 我们需要 MeshFilter 来获取网格；
    private MeshFilter meshFilter;
    private Mesh mesh;

    // 我们需要追踪我们的顶点。
    // 这意味着不仅是它们当前的状态，还有它们的原始位置等等。
    Vector3[] initialVertices;
    Vector3[] currentVertices;

    Vector3[] vertexVelocities;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;

        // 获取我们的顶点（初始和它们的当前状态（因为我们还没有做任何事情，所以是初始状态））
        initialVertices = mesh.vertices;

        // 显然，我们永远不会改变顶点的实际数量，因此这两个数组将始终具有相同的长度
        currentVertices = new Vector3[initialVertices.Length];
        vertexVelocities = new Vector3[initialVertices.Length];
        for (int i = 0; i < initialVertices.Length; i++)
        {
            currentVertices[i] = initialVertices[i];
        }
    }

    private void Update()
    {
        UpdateVertices();
    }

    private void UpdateVertices()
    {
        // 我们正在循环遍历每个顶点，根据它们的速度更新它们。
        for (int i = 0; i < currentVertices.Length; i++)
        {
            // 在我们将当前速度添加到顶点之前，我们需要确保考虑到我们的物体是一个软体，应该能够反弹回来
            // 为了做到这一点，我们首先计算位移值。
            // 由于我们保存了网格的初始形式，我们可以使用它来随着时间的推移恢复到初始位置
            Vector3 currentDisplacement = currentVertices[i] - initialVertices[i];
            vertexVelocities[i] -= currentDisplacement * bounceSpeed * Time.deltaTime;

            // 为了能够在某个时刻停止弹跳，我们需要随着时间减小速度。
            vertexVelocities[i] *= 1f - stiffness * Time.deltaTime;
            currentVertices[i] += vertexVelocities[i] * Time.deltaTime;

        }

        // 然后，我们需要将我们的 mesh.vertices 设置为当前的顶点
        // 以便能够看到变化。
        mesh.vertices = currentVertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

    }

    public void OnCollisionEnter(Collision other)
    {
        ContactPoint[] collisonPoints = other.contacts;
        for (int i = 0; i < collisonPoints.Length; i++)
        {
            Vector3 inputPoint = collisonPoints[i].point + (collisonPoints[i].point * .1f);
            ApplyPressureToPoint(inputPoint, fallForce);
        }
        
    }

    public void ApplyPressureToPoint(Vector3 _point, float _pressure)
    {
        // 我们需要循环遍历每一个顶点，并对其施加压力。
        for (int i = 0; i < currentVertices.Length; i++)
        {
            ApplyPressureToVertex(i, _point, _pressure);
        }
    }

    public void ApplyPressureToVertex(int _index, Vector3 _position, float _pressure)
    {
        // 为了知道我们需要对每个顶点施加多少压力，我们需要计算我们的顶点与我们的延伸手指（鼠标）触碰网格的点之间的距离
        Vector3 distanceVerticePoint = currentVertices[_index] - transform.InverseTransformPoint(_position);

        // 现在开始有趣的物理部分... 我们需要利用反比例平方定律
        // 我们通过将压力除以距离的平方加上 1 来做到这一点，获得一个适应的压力
        float adaptedPressure = _pressure / (1f + distanceVerticePoint.sqrMagnitude);

        // 现在剩下的就是使用这个压力来计算顶点的速度了。
        float velocity = adaptedPressure * Time.deltaTime;
        // 现在我们的速度仍然需要一个方向，我们可以通过使用之前的归一化距离顶点点来计算这个方向
        vertexVelocities[_index] += distanceVerticePoint.normalized * velocity;
    }
}
