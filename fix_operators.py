import os
import re

def fix_file(filepath):
    """Corrige tous les opérateurs ternaires et null-coalescent"""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original = content
        
        # Correction opérateurs null-coalescent (espace au lieu de ??)
        content = re.sub(r'(\w+\?(?:\.\w+\(\))?)\s+(["\w])', r'\1 ?? \2', content)
        
        # Correction opérateurs ternaires (espace au lieu de ?)
        content = re.sub(r'(\w+)\s+(["\w\(].*?)\s*:\s*', r'\1 ? \2 : ', content)
        
        # Correction opérateurs ternaires sur plusieurs lignes
        content = re.sub(r'(\w+)\s*\n\s*([^\n:]+)\n\s*:\s*', r'\1 ? \2 : ', content)
        
        if content != original:
            with open(filepath, 'w', encoding='utf-8', newline='') as f:
                f.write(content)
            print(f"? {filepath}")
            return True
    except Exception as e:
        print(f"? {filepath}: {e}")
    return False

# Liste des fichiers à corriger
files = [
    "Core/Interfaces/IFilonValidator.cs",
    "Core/Services/AutoSaveService.cs",
    "Core/Services/FilonSearchService.cs",
    "Form1.cs",
    "Forms/ContactsPanel.cs",
    "Forms/FilonEditForm.cs",
    "Forms/ImportPanel.cs",
    "Forms/MineralEditForm.cs",
    "Forms/MineralsPanel.cs",
    "Forms/OcrImportForm.cs"
]

fixed = 0
for f in files:
    if fix_file(f):
        fixed += 1

print(f"\n? {fixed}/{len(files)} fichiers corrigés!")
