using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace KaylieNodeLib.Networking
{

    public abstract class SimpleBind<TIn, TOut, TNode> : FrooxEngine.ProtoFlux.Runtimes.Execution.ObjectFunctionNode<FrooxEngineContext, TOut>
        where TNode: class, INode, new()
    {
        public readonly SyncRef<INodeObjectOutput<TIn>> Input;
        public override Type NodeType => typeof(TNode);
        public TNode TypedNodeInstance { get; private set; }
        public override INode NodeInstance => TypedNodeInstance;

        public override N Instantiate<N>()
        {
            if (TypedNodeInstance != null)
                throw new InvalidOperationException("Node has already been instantiated");
            TypedNodeInstance = new();
            return TypedNodeInstance as N;
        }
        
        public override int NodeInputCount => base.NodeInputCount + 1;

        public override void ClearInstance() => TypedNodeInstance = null;

        protected override void AssociateInstanceInternal(INode node)
        {
            if (node is TNode n)
            {
                TypedNodeInstance = n;
            }
            else
            {
                throw new ArgumentException();
            }
        }
        protected override ISyncRef GetInputInternal(ref int index)
        {
            var inputInternal = base.GetInputInternal(ref index);
            if (inputInternal != null)
                return inputInternal;
            if (index == 0)
                return Input;
            index -= 1;
            return null;
        }
    }

    [Category("ProtoFlux/Runtimes/Execution/Nodes/Network")]
    public sealed class UriSchemeBind : SimpleBind<Uri, string, UriScheme>
    {
    }
    
    [NodeCategory("Network/URIs")]
    public class UriScheme : ObjectFunctionNode<FrooxEngineContext, string>
    {
        public ObjectArgument<Uri> Uri;
        
        protected override string Compute(FrooxEngineContext context)
        {
            var uri = 0.ReadObject<Uri>(context);
            return uri.Scheme;
        }
    }
    
    [Category("ProtoFlux/Runtimes/Execution/Nodes/Network")]
    public sealed class UriAbsolutePathBind : SimpleBind<Uri, string, UriAbsolutePath>
    {
    }
    
    [NodeCategory("Network/URIs")]
    public class UriAbsolutePath : ObjectFunctionNode<FrooxEngineContext, string>
    {
        public ObjectArgument<Uri> Uri;
        
        protected override string Compute(FrooxEngineContext context)
        {
            var uri = 0.ReadObject<Uri>(context);
            return uri.AbsolutePath;
        }
    }
    
    [Category("ProtoFlux/Runtimes/Execution/Nodes/Network")]
    public sealed class UriHostBind : SimpleBind<Uri, string, UriHost>
    {
    }
    
    [NodeCategory("Network/URIs")]
    public class UriHost : ObjectFunctionNode<FrooxEngineContext, string>
    {
        public ObjectArgument<Uri> Uri;
        
        protected override string Compute(FrooxEngineContext context)
        {
            var uri = 0.ReadObject<Uri>(context);
            return uri.Host;
        }
    }
    
    [Category("ProtoFlux/Runtimes/Execution/Nodes/Network")]
    public sealed class UriPortBind : SimpleBind<Uri, string, UriPort>
    {
    }
    
    [NodeCategory("Network/URIs")]
    public class UriPort : ValueFunctionNode<FrooxEngineContext, int>
    {
        public ObjectArgument<Uri> Uri;
        
        protected override int Compute(FrooxEngineContext context)
        {
            var uri = 0.ReadObject<Uri>(context);
            return uri.Port;
        }
    }
    
    [Category("ProtoFlux/Runtimes/Execution/Nodes/Network")]
    public sealed class UriQueryBind : SimpleBind<Uri, string, UriQuery>
    {
    }
    
    [NodeCategory("Network/URIs")]
    public class UriQuery : ObjectFunctionNode<FrooxEngineContext, string>
    {
        public ObjectArgument<Uri> Uri;
        
        protected override string Compute(FrooxEngineContext context)
        {
            var uri = 0.ReadObject<Uri>(context);
            return uri.Query;
        }
    }

    [NodeCategory("Network/URIs")]
    public class UriFromParts : ObjectFunctionNode<FrooxEngineContext, Uri>
    {
        public ObjectArgument<string> Scheme;
        public ObjectArgument<string> Host;
        public ValueArgument<int> Port;
        public ObjectArgument<string> Path;
        public ObjectArgument<string> Query;
        
        protected override Uri Compute(FrooxEngineContext context)
        {
            var scheme = 0.ReadObject<string>(context);
            var host = 1.ReadObject<string>(context);
            var port = 2.ReadValue<int>(context);
            var path = 3.ReadObject<string>(context);
            var query = 4.ReadObject<string>(context);
            var builder = new UriBuilder
            {
                Scheme = scheme,
                Host = host,
                Port = port,
                Path = path,
                Query = query
            };

            return builder.Uri;
        }
    }
}