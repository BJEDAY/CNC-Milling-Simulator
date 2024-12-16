<p align="center">
  <img src="https://github.com/user-attachments/assets/0a7f0472-3025-443d-a6d9-00e281cf7ac4" width =700 height=auto>
</p>

The application simulates graphical 3C milling process while in the meantime is veryfing correctness
and quality of the paths and detects errors such as milling with non-cutting part, milling below
minimum height or lowering the flat cutter directly onto the material. To optimize the proccess all of
that is calculated inside shaders on GPU and then rendered to height map texture that is used to
display material.

<p align="center">
  <img src="https://github.com/user-attachments/assets/fe0442f7-094f-4277-9620-360fa867d62b" width =700 height=auto>
</p>

User can choose parameters such as the size and division of the material mesh separately for two
axes, display current path, set certain simulation speed time or make it instant and futhermore
freedomly control camera (it’s a combination of classic fps camera with orbital movement). Given
results are illuminated using Phong method and material has one of three available wood textures
applied on itself.

<p align="center">
  <img src="https://github.com/user-attachments/assets/d8d74c79-2656-498d-8236-46dcd4d3def6" width =700 height=auto>
</p>

<p align="center">
  <img src="https://github.com/user-attachments/assets/e696a6a4-0181-4d54-94be-5fea1ac9f133" width =700 height=auto>
</p>

Technology: C# | WPF | OpenTK
