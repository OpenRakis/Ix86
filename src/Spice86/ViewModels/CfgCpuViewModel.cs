﻿namespace Spice86.ViewModels;

using Avalonia.Threading;

using AvaloniaGraphControl;

using CommunityToolkit.Mvvm.ComponentModel;

using Spice86.Core.Emulator.CPU.CfgCpu;
using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction;
using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.SelfModifying;
using Spice86.Core.Emulator.VM;
using Spice86.Infrastructure;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Interfaces;

using System.Diagnostics;

public partial class CfgCpuViewModel : ViewModelBase {
    private readonly IPerformanceMeasurer _performanceMeasurer;
    private readonly ExecutionContextManager _executionContextManager;

    [ObservableProperty]
    private int _maxNodesToDisplay = 200;

    [ObservableProperty]
    private Graph? _graph;

    [ObservableProperty]
    private long _numberOfNodes;

    [ObservableProperty]
    private long _averageNodeTime;

    private bool _isPaused;

    [ObservableProperty]
    private bool _isVisible;

    public CfgCpuViewModel(ExecutionContextManager executionContextManager, IPauseHandler pauseHandler, IPerformanceMeasurer performanceMeasurer) {
        _executionContextManager = executionContextManager;
        _performanceMeasurer = performanceMeasurer;
        pauseHandler.Pausing += OnPausing;
        _isPaused = pauseHandler.IsPaused;
        DispatcherTimerStarter.StartNewDispatcherTimer(TimeSpan.FromMilliseconds(400), DispatcherPriority.Normal, UpdateCurrentGraph);
    }

    private void OnPausing() {
        _isPaused = true;
        Graph = null;
    }

    partial void OnMaxNodesToDisplayChanging(int value) => Graph = null;

    private async void UpdateCurrentGraph(object? sender, EventArgs e) {
        if (!_isPaused || Graph is not null) {
            return;
        }

        await Task.Run(async () => {
            ICfgNode? nodeRoot = _executionContextManager.CurrentExecutionContext?.LastExecuted;
            if (nodeRoot is null) {
                return;
            }
            
            long localNumberOfNodes = 0;
            Graph currentGraph = new();
            Queue<ICfgNode> queue = new();
            queue.Enqueue(nodeRoot);
            HashSet<ICfgNode> visitedNodes = new();
            HashSet<(int, int)> existingEdges = new();
            Stopwatch stopwatch = new();
            while (queue.Count > 0 && localNumberOfNodes < MaxNodesToDisplay) {
                ICfgNode node = queue.Dequeue();
                if (visitedNodes.Contains(node)) {
                    continue;
                }
                visitedNodes.Add(node);
                stopwatch.Restart();
                foreach (ICfgNode successor in node.Successors) {
                    (int, int) edgeKey = GenerateEdgeKey(node, successor);
                    if (!existingEdges.Contains(edgeKey)) {
                        currentGraph.Edges.Add(CreateEdge(node, successor));
                        existingEdges.Add(edgeKey);
                    }
                    if (!visitedNodes.Contains(successor)) {
                        queue.Enqueue(successor);
                    }
                }
                foreach (ICfgNode predecessor in node.Predecessors) {
                    (int, int) edgeKey = GenerateEdgeKey(predecessor, node);
                    if (!existingEdges.Contains(edgeKey)) {
                        currentGraph.Edges.Add(CreateEdge(predecessor, node));
                        existingEdges.Add(edgeKey);
                    }
                    if (!visitedNodes.Contains(predecessor)) {
                        queue.Enqueue(predecessor);
                    }
                }
                stopwatch.Stop();
                _performanceMeasurer.UpdateValue(stopwatch.ElapsedMilliseconds);
                localNumberOfNodes++;
            }

            long averageNodeTime = _performanceMeasurer.ValuePerMillisecond;

            await Dispatcher.UIThread.InvokeAsync(() => {
                Graph = currentGraph;
                NumberOfNodes = localNumberOfNodes;
                AverageNodeTime = averageNodeTime;
            });
        }).ConfigureAwait(false);
    }

    private Edge CreateEdge(ICfgNode node, ICfgNode successor) {
        string label = string.Empty;
        if (node is CfgInstruction cfgInstruction) {
            SegmentedAddress nextAddress = new SegmentedAddress(cfgInstruction.Address.Segment, (ushort)(cfgInstruction.Address.Offset + cfgInstruction.Length));
            if (successor.Address.ToPhysical() != nextAddress.ToPhysical()) {
                // Not direct successor, jump or call
                label = "not contiguous";
            }
        }
        if (node is DiscriminatedNode discriminatedNode) {
            Discriminator discriminator = discriminatedNode.SuccessorsPerDiscriminator.FirstOrDefault(x => x.Value == successor).Key;
            label = discriminator.ToString();
        }
        return new Edge(GenerateNodeText(node), GenerateNodeText(successor), label);
    }

    private (int, int) GenerateEdgeKey(ICfgNode node, ICfgNode successor)
        => (node.Id, successor.Id);

    private string GenerateNodeText(ICfgNode node) =>
        $"{node.Address} / {node.Id} {Environment.NewLine} {node.GetType().Name}";
}
