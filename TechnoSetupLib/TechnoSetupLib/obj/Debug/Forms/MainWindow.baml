   M S B A M L   `   `   `  ����I  ETechnoSetupLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullC&clr-namespace:TechnoSetupLib.ModelViewTechnoSetupLib.ModelView  5          TechnoSetupLib.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      <	ModelView&clr-namespace:TechnoSetupLib.ModelView     5      -   5        $��14(�5   K     � WindowStyle$
  None=�5      $��1q�6!   
������ � Title$ TechnoSetup Installer��5       � 
ResizeMode$ NoResize=�6=   ���	#FF2D2D30$	��400��5      $	��500��6    � WindowStartupLocation$ CenterScreen=�6'    � 	Resources 5	       �� MergedDictionaries �� Source    BooleanToVisibilityConverter�� 5
   
    5      �� 5      $7 1/TechnoSetupLib;component/Themes/ButtonStyle.xaml?�6%   �� 5      $9 3/TechnoSetupLib;component/Themes/ControlsTheme.xaml?�6%   %   &	        �� 5      6+   5       � DataContext 5      ,   &TechnoSetupLib.ModelView.MainModelView  5   
   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      �� 5      $
��Auto �6   .+�5   
   �� -   5        $��24��5      $��30��6   $��Right=�6(   $��0,8,34,5q�6D   $	��Top=�6V   	 � ToolTip$	 Minimize��6o   $��Stretch=�6�   .��5      �� $��2��6   $��5,8,5,0q�6�   ��6#   �� *
Foreground+
 �� RelativeSource
 ��  �� AncestorType" �� �� Mode$ FindAncestor=� �� UpdateSourceTrigger$ PropertyChanged=�6�   5#      �� 5$   
   -   5        $��24��5$      $��30��6   $��Right=�6(   $	��Top=�6D   $��5,8,5,0q�6\   $	 Close��6m   .��5%      F� }���Ш�� @  �����x� @�e  �p�L �2 �X�     �    @  �0�2 X�L Q�a  D�l��� @�X� ��� ��ZL  [f �h�~ ��� Ѩ�� h�~ � [f �^L 	 J� SnapsToDevicePixels . ��5&      �� *
Foreground+
 �� " ��$ FindAncestor=�$ PropertyChanged=�5'      5(      �� 5*   
   $	��Top=�61   $��5,43,5,0q�6I   $
��Wrap=�6\   $
��Bold"�6p   
���������6   �� *CurrentOper+6�   �� 5+   
    �� 
IsReadOnly . $��2��6D   
������  �� TextWrapping$
 Wrap=�6k   
�������$��5q�5,   %   $��0q�60   " �� VerticalScrollBarVisibility$
 Auto=�6D   ��5+      �� *ApplicationLog+5,   D   5-      5.      