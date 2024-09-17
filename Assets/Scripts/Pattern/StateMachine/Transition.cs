using UnityEngine;
using System.Collections;

public class Transition {

	public SMDelegate onTransiteIn;
	public SMDelegate onTransiteOut;

	public string triggerAction;

	public State source;
	public State target;

	public Transition( State source, State target, string triggerAction, SMDelegate onTransiteOut = null, SMDelegate onTransiteIn = null)
	{
		if (onTransiteIn != null)
		{
			this.onTransiteIn = onTransiteIn;
		}

		if (onTransiteOut != null)
		{
			this.onTransiteOut = onTransiteOut;
		}

		this.source = source;
		this.target = target;
		this.triggerAction = triggerAction;
	}

	public void Execute()
	{
		if (onTransiteOut != null)
		{
			onTransiteOut();
		}
		
		if (onTransiteIn != null)
		{
			onTransiteIn();
		}
	}

}
