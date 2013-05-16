using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimerImage
{
	public class Disposer : IDisposable
	{
		private List<IDisposable> _internalList = new List<IDisposable>();

		public T Track<T>(T item) where T : IDisposable
		{
			_internalList.Add(item);
			return item;
		}

		public void Dispose()
		{
			foreach (var item in _internalList)
			{
				item.Dispose();
			}
		}
	}
}