struct Input
{
    float3 position;
};

struct Trail 
{
    int currentNodeIdx;
};

struct Node
{
    float time;
    float3 position;
};

uint _NodeNumPerTrail;

int toNodeBufIdx(int trailIdx, int nodeIdx)
{
    nodeIdx %= _NodeNumPerTrail;
    return trailIdx * _NodeNumPerTrail + nodeIdx;
}

bool isValid(Node node)
{
    return node.time >= 0;
}