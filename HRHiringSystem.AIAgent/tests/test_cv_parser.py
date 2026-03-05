from app.tools.cv_parser import CvParser


def test_parse_text_extracts_contact_and_name() -> None:
    parser = CvParser()
    sample = b"""John Doe\nSenior Software Engineer\njohn.doe@example.com\n+1 (555) 123-4567\nPython, C#, SQL\n"""

    result = parser.parse(sample, "resume.txt")

    assert result.first_name == "John"
    assert result.last_name == "Doe"
    assert result.email == "john.doe@example.com"
    assert result.phone is not None
