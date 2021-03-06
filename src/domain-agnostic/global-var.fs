namespace DomainAgnostic

open System


module Globals =

    type GlobalVar<'T>(init: 'T) =

        let runloop (agent: Agent<('T -> 'T) * AsyncReplyChannel<'T>>) state =
            async {
                let! update, ch = Agent.Receive agent
                let next = update state
                ch.Reply next
                return next
            }

        let agent =
            let a = Agent.Supervised Agent.DefaultErrHandle runloop init Async.DefaultCancellationToken
            a.Start()
            a

        interface IDisposable with
            member __.Dispose() = agent |> IDisposable.DisposeOf

        member __.Get() = agent.PostAndAsyncReply(fun ch -> (id, ch))

        member __.Put state =
            agent.PostAndAsyncReply(fun ch -> (constantly state, ch))
            |> Async.Ignore

        member __.Update callback = agent.PostAndAsyncReply(fun ch -> (callback, ch))
