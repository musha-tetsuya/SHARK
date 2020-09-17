using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class TaskExtensions
{
    public static async void Throw(this Task task)
    {
        try
        {
            await task;
        }
        catch(Exception e) when (!(e is OperationCanceledException))
        {
            Debug.LogError(e);
        }
    }

    public static TaskYieldInstruction ToCoroutine(this Task task)
    {
        if (task == null)
        {
            throw new NullReferenceException();
        }

        return new TaskYieldInstruction(task);
    }

    public static TaskYieldInstruction<T> ToCoroutine<T>(this Task<T> task)
    {
        if (task == null)
        {
            throw new NullReferenceException();
        }

        return new TaskYieldInstruction<T>(task);
    }

    public class TaskYieldInstruction : CustomYieldInstruction
    {
        public Task Task { get; private set; }

        public override bool keepWaiting
        {
            get
            {
                if (Task.Exception != null)
                    throw Task.Exception;

                return !Task.IsCompleted;
            }
        }

        public TaskYieldInstruction(Task task)
        {
            Task = task ?? throw new ArgumentNullException("task");
        }
    }

    public class TaskYieldInstruction<T> : TaskYieldInstruction
    {
        public new Task<T> Task { get; private set; }

        public T Result
        {
            get { return Task.Result; }
        }

        public TaskYieldInstruction(Task<T> task)
            : base(task)
        {
            Task = task;
        }
    }
}
