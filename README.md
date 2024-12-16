![image](https://github.com/user-attachments/assets/0a7f0472-3025-443d-a6d9-00e281cf7ac4)

The application simulates graphical 3C milling process while in the meantime is veryfing correctness
and quality of the paths and detects errors such as milling with non-cutting part, milling below
minimum height or lowering the flat cutter directly onto the material. To optimize the proccess all of
that is calculated inside shaders on GPU and then rendered to height map texture that is used to
display material.

