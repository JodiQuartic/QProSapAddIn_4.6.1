﻿/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2018 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;

namespace Xceed.Wpf.Toolkit.Media.Animation
{
  public delegate T IterativeAnimationEquationDelegate<T>( TimeSpan currentTime, T from, T to, TimeSpan duration );
}
