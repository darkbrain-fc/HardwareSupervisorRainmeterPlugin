/*
    HardwareSupervisorRainmeterPlugin
    Copyright(C) 2021 Dino Puller

    This program is free software; you can redistribute itand /or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or (at
	your option) any later version.

	This program is distributed in the hope that it will be useful, but
	WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the GNU
	General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111 - 1307
	USA.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public class SortableBindingList<T> : BindingList<T>
{
    private bool mSortedValue;
    private ListSortDirection mSortDirectionValue;
    private PropertyDescriptor SortPropertyValue;

    public SortableBindingList()
    {
    }

    public SortableBindingList(IList<T> list)
    {
        foreach (object o in list)
        {
            this.Add((T)o);
        }
    }

    protected override void ApplySortCore(PropertyDescriptor prop,
        ListSortDirection direction)
    {
        Type interfaceType = prop.PropertyType.GetInterface("IComparable");

        if (interfaceType == null && prop.PropertyType.IsValueType)
        {
            Type underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);

            if (underlyingType != null)
            {
                interfaceType = underlyingType.GetInterface("IComparable");
            }
        }

        if (interfaceType != null)
        {
            SortPropertyValue = prop;
            mSortDirectionValue = direction;

            IEnumerable<T> query = base.Items;

            if (direction == ListSortDirection.Ascending)
            {
                query = query.OrderBy(i => prop.GetValue(i));
            }
            else
            {
                query = query.OrderByDescending(i => prop.GetValue(i));
            }

            int newIndex = 0;
            foreach (object item in query)
            {
                this.Items[newIndex] = (T)item;
                newIndex++;
            }

            mSortedValue = true;
            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
        else
        {
            throw new NotSupportedException("Cannot sort by " + prop.Name +
                ". This" + prop.PropertyType.ToString() +
                " does not implement IComparable");
        }
    }

    protected override PropertyDescriptor SortPropertyCore
    {
        get { return SortPropertyValue; }
    }

    protected override ListSortDirection SortDirectionCore
    {
        get { return mSortDirectionValue; }
    }

    protected override bool SupportsSortingCore
    {
        get { return true; }
    }

    protected override bool IsSortedCore
    {
        get { return mSortedValue; }
    }
}