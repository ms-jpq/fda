namespace DomainAgnostic

open System


module Globals =

    type GlobalVar<'T>(init: 'T) =

        let runloop (agent: Agent<('T -> 'T) * AsyncReplyChannel<'T>>) =
            async {
                let mutable next = init
                while true do
                    let! update, ch = Agent.Receive agent
                    next <- update next
                    ch.Reply next
            }

        let agent =
            let a = Agent.Start runloop
            a

        interface IDisposable with
            member __.Dispose() = agent |> IDisposable.DisposeOf

        member __.Get() = agent.PostAndAsyncReply(fun ch -> (id, ch))

        member __.Put state =
            agent.PostAndAsyncReply(fun ch -> (constantly state, ch))
            |> Async.Ignore

        member __.Update callback = agent.PostAndAsyncReply(fun ch -> (callback, ch))
