/*
   Copyright 2026 Alexander Stärk

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace Basilisque.AspNetCore.StoplightElements.Demo.DTO;

/// <summary>
/// Represents the data transfer object for creating a new to-do item, containing only the title of the item.
/// </summary>
/// <param name="Title">The title or description of the to-do item.</param>
public record CreateTodoDto(string Title);