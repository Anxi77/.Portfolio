import os
import re
from datetime import datetime
import pytz
from github import Github

COMMIT_TYPES = {
    'feat': {'emoji': '✨', 'label': 'feature', 'description': 'New Feature'},
    'fix': {'emoji': '🐛', 'label': 'bug', 'description': 'Bug Fix'},
    'refactor': {'emoji': '♻️', 'label': 'refactor', 'description': 'Code Refactoring'},
    'docs': {'emoji': '📝', 'label': 'documentation', 'description': 'Documentation Update'},
    'test': {'emoji': '✅', 'label': 'test', 'description': 'Test Update'},
    'chore': {'emoji': '🔧', 'label': 'chore', 'description': 'Build/Config Update'},
    'style': {'emoji': '💄', 'label': 'style', 'description': 'Code Style Update'},
    'perf': {'emoji': '⚡️', 'label': 'performance', 'description': 'Performance Improvement'},
}

def parse_commit_message(message):
    """Parse commit message"""
    pattern = r'\[(.*?)\]\s*(.*?)\n\n\[body\](.*?)(?:\n\n\[todo\](.*?))?(?:\n\n\[footer\](.*?))?$'
    match = re.search(pattern, message, re.DOTALL)
    if not match:
        return None
    
    commit_type = match.group(1).lower()
    type_info = COMMIT_TYPES.get(commit_type, {'emoji': '🔍', 'label': 'other', 'description': 'Other'})
    
    return {
        'type': commit_type,
        'type_info': type_info,
        'title': match.group(2),
        'body': match.group(3),
        'todo': match.group(4),
        'footer': match.group(5)
    }

def convert_to_checkbox_list(text):
    """Convert text to checkbox list"""
    if not text:
        return ''
    
    lines = []
    for line in text.strip().split('\n'):
        line = line.strip()
        if line.startswith(('-', '*')):
            line = f"- [ ] {line[1:].strip()}"
        lines.append(line)
    
    return '\n'.join(lines)

def create_commit_section(commit_data, branch, commit_sha, author, time_string):
    """Create commit section with details tag"""
    # 본문의 각 줄에 blockquote 적용
    body_lines = [f"> {line}" for line in commit_data['body'].strip().split('\n')]
    quoted_body = '\n'.join(body_lines)
    
    # 관련 이슈가 있는 경우 blockquote 적용
    related_issues = f"\n> **Related Issues:**\n> {commit_data['footer'].strip()}" if commit_data['footer'] else ''
    
    section = f'''> <details>
> <summary>💫 {time_string} - {commit_data['title'].strip()}</summary>
>
> Type: {commit_data['type']} ({commit_data['type_info']['description']})
> Commit: `{commit_sha}`
> Author: {author}
>
{quoted_body}{related_issues}
> </details>'''
    return section

def create_section(title, content):
    """Create collapsible section"""
    if not content:
        return ''
    
    return f'''<details>
<summary>{title}</summary>

{content}
</details>'''

def parse_existing_issue(body):
    """Parse existing issue body to extract branch commits and todos"""
    # Initialize result structure
    result = {
        'branches': {},
        'todos': []
    }
    
    # Split content by sections
    sections = body.split('---')
    
    # Parse branch sections
    branch_section = sections[0] if sections else ''
    branch_blocks = re.finditer(r'<details>\s*<summary>✨\s*(\w+)</summary>(.*?)</details>', 
                              branch_section, re.DOTALL)
    
    for block in branch_blocks:
        branch_name = block.group(1)
        branch_content = block.group(2)
        result['branches'][branch_name] = branch_content.strip()
    
    # Parse todos
    if len(sections) > 1:
        todo_section = sections[1]
        todo_matches = re.finditer(r'- \[([ x])\] (.*?)(?:\n|$)', todo_section, re.MULTILINE)
        result['todos'] = [(match.group(1) != ' ', match.group(2)) for match in todo_matches]
    
    return result

def merge_todos(existing_todos, new_todos_text):
    """Merge existing todos with new ones, avoiding duplicates"""
    if not new_todos_text:
        return existing_todos
    
    # Convert existing todos to dict for easy duplicate checking
    todo_dict = {todo[1]: todo[0] for todo in existing_todos}
    
    # Parse and add new todos
    for line in new_todos_text.strip().split('\n'):
        if line.startswith('- '):
            todo_text = line[5:] if line.startswith('- [ ]') else line[2:]
            if todo_text not in todo_dict:
                todo_dict[todo_text] = False
    
    # Convert back to list of tuples
    return [(checked, text) for text, checked in todo_dict.items()]

def create_todo_section(todos):
    """Create todo section from list of (checked, text) tuples"""
    if not todos:
        return ''
    
    todo_lines = []
    for checked, text in todos:
        checkbox = '[x]' if checked else '[ ]'
        todo_lines.append(f'- {checkbox} {text}')
    
    return '\n'.join(todo_lines)

def main():
    # Initialize GitHub token and environment variables
    github_token = os.environ['GITHUB_TOKEN']
    timezone = os.environ.get('TIMEZONE', 'Asia/Seoul')
    issue_prefix = os.environ.get('ISSUE_PREFIX', '📅')
    issue_label = os.environ.get('ISSUE_LABEL', 'daily-log')
    excluded_pattern = os.environ.get('EXCLUDED_COMMITS', '^(chore|docs|style):')

    # Initialize GitHub API client
    g = Github(github_token)
    
    # Get commit information from environment variables
    repository = os.environ['GITHUB_REPOSITORY']
    repo = g.get_repo(repository)
    commit_sha = os.environ['GITHUB_SHA']
    commit = repo.get_commit(commit_sha)
    branch = os.environ['GITHUB_REF'].replace('refs/heads/', '')

    # Check for excluded commit types
    if re.match(excluded_pattern, commit.commit.message):
        print(f"Excluded commit type: {commit.commit.message}")
        return

    # Parse commit message
    commit_data = parse_commit_message(commit.commit.message)
    if not commit_data:
        print("Invalid commit message format")
        return

    # Get current time in specified timezone
    tz = pytz.timezone(timezone)
    now = datetime.now(tz)
    date_string = now.strftime('%Y-%m-%d')
    time_string = now.strftime('%H:%M:%S')

    # Get repository name from full path
    repo_name = repository.split('/')[-1]

    # Create issue title
    issue_title = f"{issue_prefix} Daily Development Log ({date_string}) - {repo_name}"

    # Create commit section
    commit_details = create_commit_section(
        commit_data,
        branch,
        commit_sha,
        commit.commit.author.name,
        time_string
    )

    # Create todo section
    todo_section = convert_to_checkbox_list(commit_data['todo']) if commit_data['todo'] else ''

    # Create full body
    body = f'''# {issue_title}

<div align="center">

## 📊 Branch Summary

</div>

<details>
<summary><h3 style="display: inline;">✨ {branch.title()}</h3></summary>

{commit_details}
</details>

<div align="center">

## 📝 Todo

</div>

{todo_section}'''

    # Search for existing issue
    issues = repo.get_issues(state='open', labels=[issue_label])
    today_issue = None
    for issue in issues:
        if issue.title == issue_title:
            today_issue = issue
            break

    if today_issue:
        # Parse existing issue
        existing_content = parse_existing_issue(today_issue.body)
        
        # Add or update branch section
        existing_content['branches'][branch.title()] = commit_details
        
        # Merge todos
        all_todos = merge_todos(existing_content['todos'], commit_data['todo'])
        
        # Create updated body
        branch_sections = []
        for branch_name, branch_content in existing_content['branches'].items():
            branch_sections.append(f'''<details>
<summary><h3 style="display: inline;">✨ {branch_name}</h3></summary>

{branch_content}
</details>''')
        
        updated_body = f'''# {issue_title}

<div align="center">

## 📊 Branch Summary

</div>

{''.join(branch_sections)}

<div align="center">

## 📝 Todo

</div>

{create_todo_section(all_todos)}'''
        
        today_issue.edit(body=updated_body)
        print(f"Updated issue #{today_issue.number}")
    else:
        # Create new issue with initial content
        new_issue = repo.create_issue(
            title=issue_title,
            body=body,
            labels=[issue_label, f"branch:{branch}", f"type:{commit_data['type_info']['label']}"]
        )
        print(f"Created new issue #{new_issue.number}")

if __name__ == '__main__':
    main() 